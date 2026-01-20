<template>
  <form class="form" @submit.prevent="onSubmit">
    <div class="form-row">
      <label for="name">Pavadinimas</label>
      <input id="name" v-model.trim="form.name" type="text" required placeholder="Užduoties pavadinimas" />
    </div>

    <div class="form-row">
      <label for="description">Aprašymas</label>
      <textarea id="description" v-model.trim="form.description" rows="3" placeholder="Trumpas aprašymas"></textarea>
    </div>

    <div class="form-row">
      <label for="dueDate">Terminas</label>
      <input id="dueDate" v-model="form.dueDate" type="datetime-local" required />
    </div>

    <div class="form-row">
      <label for="priority">Prioritetas</label>
      <select id="priority" v-model.number="form.priority">
        <option :value="0">Žemas</option>
        <option :value="1">Vidutinis</option>
        <option :value="2">Aukštas</option>
        <option :value="3">Skubus</option>
      </select>
    </div>

    <div class="actions">
      <Button type="submit" variant="primary" :disabled="loading">{{ submitLabel }}</Button>
      <Button type="button" variant="ghost" @click="$emit('cancel')">Atšaukti</Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, computed, watch } from 'vue'
import { Button } from '@/components/common'
import { toDateTimeLocal } from '@/utils/date'
import type { CreateTaskRequest, UpdateTaskRequest, Task } from '@/types'

interface Props {
  task?: Task | null
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  task: null,
  loading: false
})

const emit = defineEmits<{
  submit: [payload: CreateTaskRequest | UpdateTaskRequest]
  cancel: []
}>()

const form = reactive<Required<CreateTaskRequest>>({
  name: props.task?.name || '',
  description: props.task?.description ?? '',
  dueDate: toDateTimeLocal(props.task?.dueDate) || '',
  status: props.task ? props.task.status : 0,
  priority: 1,
  assignedUserId: null
})

const submitLabel = computed(() => (props.task ? 'Išsaugoti' : 'Sukurti'))

watch(() => props.task, (t) => {
  if (!t) return
  form.name = t.name
  form.description = t.description ?? ''
  form.dueDate = toDateTimeLocal(t.dueDate) || ''
  form.status = t.status
})

function onSubmit() {
  const payload: CreateTaskRequest | UpdateTaskRequest = {
    name: form.name,
    description: form.description || null,
    dueDate: form.dueDate,
    status: form.status,
    priority: form.priority,
    assignedUserId: form.assignedUserId
  }
  emit('submit', payload)
}
</script>

<style scoped>
/* Override for specific task form rows */
.form-row:nth-child(4) {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 640px) {
  .form-row:nth-child(4) {
    grid-template-columns: 1fr;
  }
}
</style>