<template>
  <div :class="['task-item', statusClass]">
    <div class="task-checkbox">
      <input
        type="checkbox"
        :checked="task.status === TaskStatus.Completed"
        @change="toggleStatus"
      />
    </div>
    
    <div class="task-content">
      <h4 class="task-title">{{ task.name }}</h4>
      <p v-if="task.description" class="task-description">{{ task.description }}</p>
      
      <div class="task-meta">
        <span v-if="task.assignedUser" class="task-assigned">
          <Icon name="users" class="icon" />
          {{ task.assignedUser.name }}
        </span>
        
        <span v-if="task.dueDate" :class="['task-due-date', { 'overdue': isOverdue }]">
          <Icon name="calendar" class="icon" />
          {{ formatDate(task.dueDate) }}
        </span>
        
        <span class="task-status-badge">{{ taskStatusLabel }}</span>
      </div>
    </div>
    
    <div class="task-actions">
      <Button size="sm" variant="ghost" @click="$emit('edit', task)">
        Redaguoti
      </Button>
      <Button size="sm" variant="danger" @click="$emit('delete', task)">
        Ištrinti
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Button } from '@/components/common'
import { Icon } from '@/components/icons'
import { formatDate, isExpired } from '@/utils/date'
import { TaskStatus, TaskStatusLabels, type Task } from '@/types'

interface Props {
  task: Task
}

const props = defineProps<Props>()

const emit = defineEmits<{
  edit: [task: Task]
  delete: [task: Task]
  'update-status': [task: Task, status: TaskStatus]
}>()

const statusClass = computed(() => {
  return `task-status-${props.task.status}`
})

const taskStatusLabel = computed(() => {
  return TaskStatusLabels[props.task.status]
})

const isOverdue = computed(() => {
  return props.task.dueDate && 
         isExpired(props.task.dueDate) && 
         props.task.status !== TaskStatus.Completed
})

function toggleStatus() {
  const newStatus = props.task.status === TaskStatus.Completed 
    ? TaskStatus.InProgress 
    : TaskStatus.Completed
  
  emit('update-status', props.task, newStatus)
  
}
</script>

<style scoped>
.task-item {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  background: white;
  border-radius: 0.5rem;
  border: 1px solid #e5e7eb;
  transition: all 0.2s;
}

.task-item:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.task-status-2 {
  opacity: 0.7;
}

.task-status-2 .task-title {
  text-decoration: line-through;
}

.task-checkbox input {
  width: 1.25rem;
  height: 1.25rem;
  cursor: pointer;
}

.task-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.task-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.task-description {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0;
}

.task-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.task-meta > span {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.icon {
  width: 1rem;
  height: 1rem;
}

.task-due-date.overdue {
  color: var(--danger);
  font-weight: 500;
}

.task-status-badge {
  padding: 0.125rem 0.5rem;
  border-radius: 9999px;
  background-color: var(--gray-100);
  color: var(--text-secondary);
  font-size: 0.75rem;
}

.task-actions {
  display: flex;
  gap: 0.5rem;
}

@media (max-width: 640px) {
  .task-item {
    flex-direction: column;
  }
  
  .task-actions {
    width: 100%;
  }
  
  .task-actions button {
    flex: 1;
  }
}
</style>
