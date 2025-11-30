import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { activitiesAPI } from '@/api'
import type { Activity, CreateActivityRequest, UpdateActivityRequest, User } from '@/types'

/**
 * Veiklų store - valdymas veiklų hierarchijos pagrindo
 * Veikla yra aukščiausias hierarchijos lygis: Veikla → Užduotys → Išlaidos
 */
export const useActivitiesStore = defineStore('activities', () => {
  // State
  const activities = ref<Activity[]>([])
  const currentActivity = ref<Activity | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const sortedActivities = computed(() => {
    return [...activities.value].sort((a, b) => 
      new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
    )
  })

  const upcomingActivities = computed(() => {
    const now = new Date()
    return sortedActivities.value.filter(a => new Date(a.startDate) >= now)
  })

  const pastActivities = computed(() => {
    const now = new Date()
    return sortedActivities.value.filter(a => new Date(a.startDate) < now)
  })

  const activitiesCount = computed(() => activities.value.length)

  // Getter function for single activity
  const getActivity = (id: number) => {
    return activities.value.find(a => a.id === id) || currentActivity.value
  }

  // Actions
  async function fetchActivities(forceRefresh = false) {
    if (activities.value.length > 0 && !forceRefresh) {
      return activities.value
    }

    loading.value = true
    error.value = null
    
    try {
      const response = await activitiesAPI.getAll()
      
      if (response.ok && response.data) {
        activities.value = response.data
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko gauti veiklų'
        return []
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant veiklas'
      console.error('Fetch activities error:', e)
      return []
    } finally {
      loading.value = false
    }
  }

  async function fetchActivity(id: number, forceRefresh = false) {
    if (currentActivity.value?.id === id && !forceRefresh) {
      return currentActivity.value
    }

    loading.value = true
    error.value = null
    
    try {
      const response = await activitiesAPI.getById(id)
      
      if (response.ok && response.data) {
        currentActivity.value = response.data
        
        // Atnaujinti cache'e
        const index = activities.value.findIndex(a => a.id === id)
        if (index !== -1) {
          activities.value[index] = response.data
        }
        
        return response.data
      } else {
        error.value = response.error?.message || 'Veikla nerasta'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant veiklą'
      console.error('Fetch activity error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function createActivity(data: CreateActivityRequest) {
    loading.value = true
    error.value = null
    
    try {
      const response = await activitiesAPI.create(data)
      
      if (response.ok && response.data) {
        activities.value.push(response.data)
        currentActivity.value = response.data
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko sukurti veiklos'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida kuriant veiklą'
      console.error('Create activity error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateActivity(id: number, data: UpdateActivityRequest) {
    loading.value = true
    error.value = null
    
    try {
      const response = await activitiesAPI.update(id, data)
      
      // Backend grąžina 204 NoContent sėkmės atveju – refetch
      if (response.ok) {
        const refreshed = await activitiesAPI.getById(id)
        if (refreshed.ok && refreshed.data) {
          const index = activities.value.findIndex(a => a.id === id)
          if (index !== -1) {
            activities.value[index] = refreshed.data
          }
          if (currentActivity.value?.id === id) {
            currentActivity.value = refreshed.data
          }
          return refreshed.data
        }
        // Jei refetch nepavyko, bent jau nekelti klaidos – laikyti sėkme
        return activities.value.find(a => a.id === id) || null
      } else {
        error.value = response.error?.message || 'Nepavyko atnaujinti veiklos'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida atnaujinant veiklą'
      console.error('Update activity error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function deleteActivity(id: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await activitiesAPI.delete(id)
      
      if (response.ok) {
        // Pašalinti iš sąrašo
        activities.value = activities.value.filter(a => a.id !== id)
        
        // Išvalyti current
        if (currentActivity.value?.id === id) {
          currentActivity.value = null
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko ištrinti veiklos'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida trinant veiklą'
      console.error('Delete activity error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function removeParticipant(activityId: number, userId: number) {
    try {
      const response = await activitiesAPI.removeParticipant(activityId, userId)
      
      if (response.ok) {
        // Refresh activity data to get updated participants
        await fetchActivity(activityId, true)
        return true
      }
      
      return false
    } catch (e) {
      console.error('Remove participant error:', e)
      return false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentActivity() {
    currentActivity.value = null
  }

  function reset() {
    activities.value = []
    currentActivity.value = null
    loading.value = false
    error.value = null
  }

  return {
    // State
    activities,
    currentActivity,
    loading,
    error,
    
    // Getters
    sortedActivities,
    upcomingActivities,
    pastActivities,
    activitiesCount,
    getActivity,
    
    // Actions
    fetchActivities,
    fetchActivity,
    createActivity,
    updateActivity,
    deleteActivity,
    removeParticipant,
    clearError,
    clearCurrentActivity,
    reset
  }
})
