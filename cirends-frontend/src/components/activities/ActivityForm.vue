<template>
  <form @submit.prevent="handleSubmit" class="activity-form">
    <Input
      v-model="formData.name"
      label="Pavadinimas"
      :required="true"
      :error="errors.name"
      placeholder="Pvz.: Kelionė prie ežero"
    />
    
    <Input
      v-model="formData.description"
      label="Aprašymas"
      type="textarea"
      :rows="4"
      :error="errors.description"
      placeholder="Aprašykite veiklą..."
    />
    
    <div class="form-row">
      <Input
        v-model="formData.startDate"
        label="Pradžios data"
        type="date"
        :required="true"
        :error="errors.startDate"
      />
      
      <Input
        v-model="formData.endDate"
        label="Pabaigos data"
        type="date"
        :required="true"
        :error="errors.endDate"
      />
    </div>
    
    <Input
      v-model="formData.location"
      label="Vieta"
      :error="errors.location"
      placeholder="Pvz.: Trakai"
    />
    
    <div class="form-actions">
      <Button type="button" variant="ghost" @click="$emit('cancel')">
        Atšaukti
      </Button>
      <Button type="submit" :loading="loading">
        {{ isEdit ? 'Atnaujinti' : 'Sukurti' }}
      </Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, computed } from 'vue'
import { Input, Button } from '@/components/common'
import { validateRequired, validateDateRange } from '@/utils/validation'
import type { Activity, CreateActivityRequest } from '@/types'

interface Props {
  activity?: Activity | null
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  submit: [data: CreateActivityRequest]
  cancel: []
}>()

const isEdit = computed(() => !!props.activity)

const formData = reactive({
  name: props.activity?.name || '',
  description: props.activity?.description || '',
  startDate: props.activity?.startDate?.split('T')[0] || '',
  endDate: props.activity?.endDate?.split('T')[0] || '',
  location: props.activity?.location || ''
})

const errors = reactive({
  name: '',
  description: '',
  startDate: '',
  endDate: '',
  location: ''
})

function validateForm(): boolean {
  // Reset errors
  errors.name = ''
  errors.description = ''
  errors.startDate = ''
  errors.endDate = ''
  errors.location = ''
  
  let isValid = true
  
  if (!validateRequired(formData.name)) {
    errors.name = 'Pavadinimas privalomas'
    isValid = false
  }
  
  if (!validateRequired(formData.startDate)) {
    errors.startDate = 'Pradžios data privaloma'
    isValid = false
  }
  
  if (!validateRequired(formData.endDate)) {
    errors.endDate = 'Pabaigos data privaloma'
    isValid = false
  } else if (formData.endDate && !validateDateRange(formData.startDate, formData.endDate)) {
    errors.endDate = 'Pabaigos data turi būti po pradžios datos'
    isValid = false
  }
  
  return isValid
}

function handleSubmit() {
  if (!validateForm()) return
  
  const data: CreateActivityRequest = {
    name: formData.name,
    description: formData.description,
    startDate: formData.startDate,
    endDate: formData.endDate, // dabar privaloma
    location: formData.location || null
  }
  
  emit('submit', data)
}
</script>

<style scoped>
.activity-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
}

@media (max-width: 640px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
