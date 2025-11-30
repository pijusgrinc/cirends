<template>
  <div class="empty-state">
    <div v-if="icon" class="empty-state-icon" v-html="icon"></div>
    <div v-else class="empty-state-icon-default">
      <Icon name="info" size="64" />
    </div>
    
    <h3 class="empty-state-title">{{ title }}</h3>
    <p v-if="description" class="empty-state-description">{{ description }}</p>
    
    <div v-if="$slots.default || actionText" class="empty-state-action">
      <slot>
        <Button v-if="actionText" @click="$emit('action')">
          {{ actionText }}
        </Button>
      </slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import Button from './Button.vue'
import { Icon } from '@/components/icons'

interface Props {
  title: string
  description?: string
  icon?: string
  actionText?: string
}

defineProps<Props>()

defineEmits<{
  action: []
}>()
</script>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1.5rem;
  text-align: center;
}

.empty-state-icon,
.empty-state-icon-default {
  width: 4rem;
  height: 4rem;
  color: #9ca3af;
  margin-bottom: 1rem;
}

.empty-state-icon-default svg {
  width: 100%;
  height: 100%;
}

.empty-state-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.5rem 0;
}

.empty-state-description {
  font-size: 0.875rem;
  color: #6b7280;
  max-width: 28rem;
  margin: 0 0 1.5rem 0;
}

.empty-state-action {
  margin-top: 1rem;
}
</style>
