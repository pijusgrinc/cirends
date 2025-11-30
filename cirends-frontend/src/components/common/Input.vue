<template>
  <div :class="['input-wrapper', { 'input-error': error, 'input-disabled': disabled }]">
    <label v-if="label" :for="inputId" class="input-label">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    
    <div class="input-container">
      <span v-if="icon" class="input-icon" v-html="icon"></span>
      
      <input
        v-if="type !== 'textarea'"
        :id="inputId"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        :min="min"
        :max="max"
        :step="step"
        :class="['input-field', { 'input-with-icon': icon }]"
        @input="handleInput"
        @blur="handleBlur"
        @focus="handleFocus"
      />
      
      <textarea
        v-else
        :id="inputId"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        :rows="rows"
        :class="['input-field', 'input-textarea']"
        @input="handleInput"
        @blur="handleBlur"
        @focus="handleFocus"
      ></textarea>
    </div>
    
    <span v-if="error" class="input-error-message">{{ error }}</span>
    <span v-else-if="hint" class="input-hint">{{ hint }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue: string | number | null
  label?: string
  type?: 'text' | 'email' | 'password' | 'number' | 'date' | 'datetime-local' | 'textarea'
  placeholder?: string
  error?: string
  hint?: string
  disabled?: boolean
  required?: boolean
  icon?: string
  min?: string | number
  max?: string | number
  step?: string | number
  rows?: number
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  disabled: false,
  required: false,
  rows: 3
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number]
  blur: []
  focus: []
}>()

const inputId = computed(() => `input-${Math.random().toString(36).substr(2, 9)}`)

function handleInput(event: Event) {
  const target = event.target as HTMLInputElement | HTMLTextAreaElement
  const value = props.type === 'number' ? Number(target.value) : target.value
  emit('update:modelValue', value)
}

function handleBlur() {
  emit('blur')
}

function handleFocus() {
  emit('focus')
}
</script>

<style scoped>
.input-wrapper {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.input-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text-primary);
}

.input-container {
  position: relative;
}

.input-icon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  width: 1.25rem;
  height: 1.25rem;
  color: var(--text-secondary);
  pointer-events: none;
}

.input-field {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 0.375rem;
  font-size: 1rem;
  transition: all 0.2s;
  background-color: var(--white);
  color: var(--text-primary);
}

.input-field:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.input-with-icon {
  padding-left: 2.5rem;
}

.input-textarea {
  resize: vertical;
  min-height: 4rem;
}

.input-error .input-field {
  border-color: var(--danger);
}

.input-error .input-field:focus {
  border-color: var(--danger);
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.input-disabled .input-field {
  background-color: var(--gray-100);
  cursor: not-allowed;
  opacity: 0.6;
}

.input-error-message {
  font-size: 0.875rem;
  color: var(--danger);
}

.input-hint {
  font-size: 0.875rem;
  color: var(--text-secondary);
}
</style>
