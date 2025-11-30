<template>
  <button
    :class="buttonClasses"
    :disabled="disabled || loading"
    :type="type"
    @click="handleClick"
  >
    <span v-if="loading" class="spinner"></span>
    <span v-if="icon && !loading" class="btn-icon" v-html="icon"></span>
    <span v-if="!loading || showTextWhileLoading"><slot /></span>
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  variant?: 'primary' | 'secondary' | 'danger' | 'success' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  loading?: boolean
  disabled?: boolean
  fullWidth?: boolean
  type?: 'button' | 'submit' | 'reset'
  icon?: string
  showTextWhileLoading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  loading: false,
  disabled: false,
  fullWidth: false,
  type: 'button',
  showTextWhileLoading: false
})

const emit = defineEmits<{
  click: [event: MouseEvent]
}>()

const buttonClasses = computed(() => [
  'btn',
  `btn-${props.variant}`,
  `btn-${props.size}`,
  {
    'btn-loading': props.loading,
    'btn-disabled': props.disabled,
    'btn-full-width': props.fullWidth
  }
])

function handleClick(event: MouseEvent) {
  if (!props.disabled && !props.loading) {
    emit('click', event)
  }
}
</script>

<style scoped>
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  font-weight: 500;
  border-radius: 0.375rem;
  transition: all 0.2s;
  cursor: pointer;
  border: 1px solid transparent;
}

.btn:focus {
  outline: 2px solid transparent;
  outline-offset: 2px;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.3);
}

/* Sizes */
.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
}

.btn-md {
  padding: 0.5rem 1rem;
  font-size: 1rem;
}

.btn-lg {
  padding: 0.75rem 1.5rem;
  font-size: 1.125rem;
}

/* Variants */
.btn-primary {
  background-color: var(--primary);
  color: white;
}

.btn-primary:hover:not(.btn-disabled):not(.btn-loading) {
  background-color: var(--primary-dark);
}

.btn-secondary {
  background-color: var(--gray-600);
  color: white;
}

.btn-secondary:hover:not(.btn-disabled):not(.btn-loading) {
  background-color: var(--gray-700);
}

.btn-danger {
  background-color: var(--danger);
  color: white;
}

.btn-danger:hover:not(.btn-disabled):not(.btn-loading) {
  background-color: var(--danger-dark);
}

.btn-success {
  background-color: var(--success);
  color: white;
}

.btn-success:hover:not(.btn-disabled):not(.btn-loading) {
  background-color: var(--success-dark);
}

.btn-ghost {
  background-color: transparent;
  color: #374151;
  border-color: #d1d5db;
}

.btn-ghost:hover:not(.btn-disabled):not(.btn-loading) {
  background-color: #f3f4f6;
}

/* States */
.btn-disabled,
.btn-loading {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-full-width {
  width: 100%;
}

/* Spinner */
.spinner {
  width: 1em;
  height: 1em;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.btn-icon {
  width: 1.25em;
  height: 1.25em;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
</style>
