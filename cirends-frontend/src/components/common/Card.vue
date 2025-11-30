<template>
  <div
    :class="['card', cardClasses]"
    @click="handleClick"
    :role="props.clickable ? 'button' : undefined"
    :tabindex="props.clickable ? 0 : undefined"
    @keydown.enter.prevent="handleClick"
    @keydown.space.prevent="handleClick"
  >
    <div v-if="$slots.header || title" class="card-header">
      <h3 v-if="title" class="card-title">{{ title }}</h3>
      <slot name="header"></slot>
    </div>
    
    <div class="card-body">
      <slot></slot>
    </div>
    
    <div v-if="$slots.footer" class="card-footer">
      <slot name="footer"></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title?: string
  variant?: 'default' | 'bordered' | 'elevated'
  padding?: 'none' | 'sm' | 'md' | 'lg'
  hoverable?: boolean
  clickable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  padding: 'md',
  hoverable: false,
  clickable: false
})

const emit = defineEmits<{
  click: []
}>()

const cardClasses = computed(() => [
  `card-${props.variant}`,
  `card-padding-${props.padding}`,
  {
    'card-hoverable': props.hoverable,
    'card-clickable': props.clickable
  }
])

function handleClick() {
  if (props.clickable) {
    emit('click')
  }
}
</script>

<style scoped>
.card {
  background-color: var(--white);
  border-radius: 0.75rem;
  overflow: hidden;
}

/* Variants */
.card-default {
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--border-color);
}

.card-bordered {
  border: 1px solid var(--border-color);
}

.card-elevated {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  border: none;
}

/* Padding */
.card-padding-none .card-body {
  padding: 0;
}

.card-padding-sm .card-body {
  padding: 0.75rem;
}

.card-padding-md .card-body {
  padding: 1.25rem;
}

.card-padding-lg .card-body {
  padding: 2rem;
}

/* States */
.card-hoverable {
  transition: var(--transition);
}

.card-hoverable:hover {
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  transform: translateY(-2px);
}

.card-clickable {
  cursor: pointer;
}

/* Header */
.card-header {
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--border-color);
  background-color: var(--gray-50);
}

.card-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

/* Footer */
.card-footer {
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--border-color);
  background-color: var(--gray-50);
}
</style>
