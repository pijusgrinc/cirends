<template>
  <Card 
    :hoverable="true" 
    :clickable="true"
    @click="$emit('click', activity)"
    class="activity-card"
  >
    <div class="activity-card-content">
      <div class="activity-header">
        <h3 class="activity-title">{{ activity.name }}</h3>
        <span :class="['activity-status', statusClass]">
          {{ statusText }}
        </span>
      </div>
      
      <p v-if="activity.description" class="activity-description">
        {{ truncateText(activity.description, 100) }}
      </p>
      
      <div class="activity-meta">
        <div class="activity-meta-item">
          <Icon name="calendar" class="icon" />
          <span>{{ formatDate(activity.startDate) }}</span>
        </div>
        
        <div v-if="activity.location" class="activity-meta-item">
          <Icon name="location" class="icon" />
          <span>{{ activity.location }}</span>
        </div>
        
        <div v-if="activity.participants?.length" class="activity-meta-item">
          <Icon name="people" class="icon" />
          <span>{{ activity.participants.length }} dalyviai</span>
        </div>
      </div>
      
      <div v-if="showActions" class="activity-actions">
        <Button 
          size="sm" 
          variant="ghost"
          @click.stop="$emit('edit', activity)"
        >
          Redaguoti
        </Button>
        <Button 
          size="sm" 
          variant="danger"
          @click.stop="$emit('delete', activity)"
        >
          Ištrinti
        </Button>
      </div>
    </div>
  </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Card, Button } from '@/components/common'
import { Icon } from '@/components/icons'
import { formatDate, isFuture, isActive } from '@/utils/date'
import type { Activity } from '@/types'

interface Props {
  activity: Activity
  showActions?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showActions: false
})

defineEmits<{
  click: [activity: Activity]
  edit: [activity: Activity]
  delete: [activity: Activity]
}>()

const statusClass = computed(() => {
  if (isFuture(props.activity.startDate)) {
    return 'status-upcoming'
  }
  if (isActive(props.activity.startDate, props.activity.endDate)) {
    return 'status-active'
  }
  return 'status-past'
})

const statusText = computed(() => {
  if (isFuture(props.activity.startDate)) {
    return 'Artėja'
  }
  if (isActive(props.activity.startDate, props.activity.endDate)) {
    return 'Vykdoma'
  }
  return 'Įvyko'
})

function truncateText(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}
</script>

<style scoped>
.activity-card-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.activity-card-content:hover {
  transform: translateY(-2px);
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.activity-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
  flex: 1;
}

.activity-status {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  white-space: nowrap;
}

.status-upcoming {
  background-color: var(--info-light);
  color: #1e40af;
}

.status-active {
  background-color: var(--success-light);
  color: var(--success-dark);
}

.status-past {
  background-color: var(--gray-100);
  color: var(--text-secondary);
}

.activity-description {
  color: var(--text-secondary);
  font-size: 0.875rem;
  margin: 0;
  line-height: 1.5;
}

.activity-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.activity-meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.icon {
  width: 1.25rem;
  height: 1.25rem;
}

.activity-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid #e5e7eb;
}
</style>
