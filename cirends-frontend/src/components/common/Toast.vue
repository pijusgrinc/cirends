<template>
  <teleport to="body">
    <transition name="toast-fade">
      <div v-if="toasts.length > 0" class="toast-container">
        <transition-group name="toast-slide">
          <div
            v-for="toast in toasts"
            :key="toast.id"
            :class="['toast', `toast-${toast.type}`]"
            @click="remove(toast.id)"
          >
            <div class="toast-icon">
              <Icon v-if="toast.type === 'success'" name="check" />
              <Icon v-else-if="toast.type === 'error'" name="close" />
              <Icon v-else-if="toast.type === 'warning'" name="warning" />
              <Icon v-else name="info" />
            </div>
            
            <div class="toast-content">
              <p class="toast-message">{{ toast.message }}</p>
            </div>
            
            <button class="toast-close" @click.stop="remove(toast.id)">
              <Icon name="close" />
            </button>
          </div>
        </transition-group>
      </div>
    </transition>
  </teleport>
</template>

<script setup lang="ts">
import { useToast } from '@/composables/useToast'
import { Icon } from '@/components/icons'

const { toasts, remove } = useToast()
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-width: 24rem;
}

.toast {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: 0.5rem;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  cursor: pointer;
  transition: all 0.2s;
  background-color: var(--white);
}

.toast:hover {
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

.toast-icon {
  flex-shrink: 0;
  width: 1.5rem;
  height: 1.5rem;
}

.toast-success {
  border-left: 4px solid var(--success);
}

.toast-success .toast-icon {
  color: var(--success);
}

.toast-error {
  border-left: 4px solid var(--danger);
}

.toast-error .toast-icon {
  color: var(--danger);
}

.toast-warning {
  border-left: 4px solid var(--warning);
}

.toast-warning .toast-icon {
  color: var(--warning);
}

.toast-info {
  border-left: 4px solid var(--info);
}

.toast-info .toast-icon {
  color: var(--info);
}

.toast-content {
  flex: 1;
}

.toast-message {
  margin: 0;
  font-size: 0.875rem;
  color: var(--text-primary);
  line-height: 1.5;
}

.toast-close {
  flex-shrink: 0;
  width: 1.25rem;
  height: 1.25rem;
  padding: 0;
  border: none;
  background: none;
  color: var(--text-secondary);
  cursor: pointer;
  transition: color 0.2s;
}

.toast-close:hover {
  color: var(--text-primary);
}

/* Animations */
.toast-fade-enter-active,
.toast-fade-leave-active {
  transition: opacity 0.3s;
}

.toast-fade-enter-from,
.toast-fade-leave-to {
  opacity: 0;
}

.toast-slide-enter-active {
  transition: all 0.3s ease-out;
}

.toast-slide-leave-active {
  transition: all 0.2s ease-in;
}

.toast-slide-enter-from {
  transform: translateX(100%);
  opacity: 0;
}

.toast-slide-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

@media (max-width: 640px) {
  .toast-container {
    left: 1rem;
    right: 1rem;
    max-width: none;
  }
}
</style>
