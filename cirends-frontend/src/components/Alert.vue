<template>
  <div class="alert" :class="`alert-${type}`" v-if="visible">
    <div class="alert-icon">
      <Icon v-if="type === 'success'" name="check-circle" />
      <Icon v-else-if="type === 'danger'" name="close" />
      <Icon v-else-if="type === 'warning'" name="warning" />
      <Icon v-else name="info" />
    </div>
    <span>{{ message }}</span>
    <button class="alert-close" @click="visible = false">
      <Icon name="close" />
    </button>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Icon } from '@/components/icons'

const props = defineProps({
  type: {
    type: String,
    default: 'info',
    validator: (value) => ['success', 'danger', 'warning', 'info'].includes(value)
  },
  message: {
    type: String,
    required: true
  },
  dismissible: {
    type: Boolean,
    default: true
  },
  autoClose: {
    type: Boolean,
    default: true
  },
  duration: {
    type: Number,
    default: 5000
  }
})

const visible = ref(true)

watch(() => props.message, () => {
  visible.value = true
  if (props.autoClose) {
    setTimeout(() => {
      visible.value = false
    }, props.duration)
  }
})

watch(visible, (newVal) => {
  if (!newVal && props.dismissible) {
    // Handle dismissal
  }
})
</script>

<style scoped>
.alert {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  border-radius: var(--border-radius);
  margin-bottom: 1rem;
  animation: slideDown 0.3s ease;
  border-left: 4px solid;
}

.alert-success {
  background: rgba(0, 208, 132, 0.1);
  color: var(--success);
  border-left-color: var(--success);
}

.alert-danger {
  background: rgba(255, 107, 107, 0.1);
  color: var(--danger);
  border-left-color: var(--danger);
}

.alert-warning {
  background: rgba(255, 217, 61, 0.1);
  color: #cc9900;
  border-left-color: var(--warning);
}

.alert-info {
  background: rgba(107, 194, 255, 0.1);
  color: var(--info);
  border-left-color: var(--info);
}

.alert-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

.alert-close {
  margin-left: auto;
  background: none;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0.6;
  transition: var(--transition);
  padding: 0;
  width: 20px;
  height: 20px;
  color: inherit;
}

.alert-close:hover {
  opacity: 1;
}

.alert-close svg {
  width: 100%;
  height: 100%;
}
</style>
