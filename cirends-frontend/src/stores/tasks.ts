import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { tasksAPI } from '@/api'
import type { Task, CreateTaskRequest, UpdateTaskRequest } from '@/types'
import { toUtcIso } from '@/utils/date'
import { TaskStatus } from '@/types'

/**
 * Užduočių store
 * Užduotis yra antras hierarchijos lygis: Veikla → Užduotys → Išlaidos
 * Kiekviena užduotis priklauso veiklai ir gali turėti išlaidų
 */
export const useTasksStore = defineStore('tasks', () => {
  // State
  const tasksByActivity = ref<Record<number, Task[]>>({})
  const currentTask = ref<Task | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const getTasks = computed(() => (activityId: number) => {
    return tasksByActivity.value[activityId] || []
  })

  const getTasksByStatus = computed(() => (activityId: number, status: TaskStatus) => {
    const tasks = tasksByActivity.value[activityId] || []
    return tasks.filter(t => t.status === status)
  })

  const getPlannedTasks = computed(() => (activityId: number) => {
    return getTasksByStatus.value(activityId, TaskStatus.Planned)
  })

  const getInProgressTasks = computed(() => (activityId: number) => {
    return getTasksByStatus.value(activityId, TaskStatus.InProgress)
  })

  const getCompletedTasks = computed(() => (activityId: number) => {
    return getTasksByStatus.value(activityId, TaskStatus.Completed)
  })

  const tasksCount = computed(() => (activityId: number) => {
    return (tasksByActivity.value[activityId] || []).length
  })

  const completedTasksCount = computed(() => (activityId: number) => {
    return getCompletedTasks.value(activityId).length
  })

  // Actions
  async function fetchTasks(activityId: number, forceRefresh = false) {
    if (tasksByActivity.value[activityId] && !forceRefresh) {
      return tasksByActivity.value[activityId]
    }

    loading.value = true
    error.value = null
    
    try {
      const response = await tasksAPI.getAll(activityId)
      
      if (response.ok && response.data) {
        tasksByActivity.value[activityId] = response.data
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko gauti užduočių'
        return []
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant užduotis'
      console.error('Fetch tasks error:', e)
      return []
    } finally {
      loading.value = false
    }
  }

  async function fetchTask(activityId: number, taskId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await tasksAPI.getById(activityId, taskId)
      
      if (response.ok && response.data) {
        currentTask.value = response.data
        
        // Atnaujinti cache'e
        if (tasksByActivity.value[activityId]) {
          const index = tasksByActivity.value[activityId].findIndex(t => t.id === taskId)
          if (index !== -1) {
            tasksByActivity.value[activityId][index] = response.data
          }
        }
        
        return response.data
      } else {
        error.value = response.error?.message || 'Užduotis nerasta'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant užduotį'
      console.error('Fetch task error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function createTask(activityId: number, data: CreateTaskRequest) {
    loading.value = true
    error.value = null
    
    try {
      const payload: CreateTaskRequest = {
        ...data,
        dueDate: data.dueDate ? toUtcIso(data.dueDate) : null
      }
      const response = await tasksAPI.create(activityId, payload)
      
      if (response.ok && response.data) {
        // Pridėti į cache
        if (!tasksByActivity.value[activityId]) {
          tasksByActivity.value[activityId] = []
        }
        tasksByActivity.value[activityId].push(response.data)
        
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko sukurti užduoties'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida kuriant užduotį'
      console.error('Create task error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateTask(activityId: number, taskId: number, data: UpdateTaskRequest) {
    loading.value = true
    error.value = null
    
    try {
      const payload: UpdateTaskRequest = {
        ...data,
        dueDate: data.dueDate ? toUtcIso(data.dueDate) : undefined
      }
      const response = await tasksAPI.update(activityId, taskId, payload)
      
      if (response.ok) {
        // Backend returns 204 No Content, so refetch the task
        const updatedTask = await fetchTask(activityId, taskId)
        return updatedTask
      } else {
        error.value = response.error?.message || 'Nepavyko atnaujinti užduoties'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida atnaujinant užduotį'
      console.error('Update task error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateTaskStatus(activityId: number, taskId: number, status: TaskStatus) {
    return updateTask(activityId, taskId, { status })
  }

  async function deleteTask(activityId: number, taskId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await tasksAPI.delete(activityId, taskId)
      
      if (response.ok) {
        // Pašalinti iš cache
        if (tasksByActivity.value[activityId]) {
          tasksByActivity.value[activityId] = tasksByActivity.value[activityId].filter(
            t => t.id !== taskId
          )
        }
        
        // Išvalyti current
        if (currentTask.value?.id === taskId) {
          currentTask.value = null
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko ištrinti užduoties'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida trinant užduotį'
      console.error('Delete task error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentTask() {
    currentTask.value = null
  }

  function clearTasksForActivity(activityId: number) {
    delete tasksByActivity.value[activityId]
  }

  function reset() {
    tasksByActivity.value = {}
    currentTask.value = null
    loading.value = false
    error.value = null
  }

  return {
    // State
    tasksByActivity,
    currentTask,
    loading,
    error,
    
    // Getters
    getTasks,
    getTasksByStatus,
    getPlannedTasks,
    getInProgressTasks,
    getCompletedTasks,
    tasksCount,
    completedTasksCount,
    
    // Actions
    fetchTasks,
    fetchTask,
    createTask,
    updateTask,
    updateTaskStatus,
    deleteTask,
    clearError,
    clearCurrentTask,
    clearTasksForActivity,
    reset
  }
})
