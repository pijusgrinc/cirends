<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg p-6 w-full max-w-md">
      <h2 class="text-xl font-bold mb-4">Keisti slaptažodį</h2>
      
      <form @submit.prevent="handleSubmit">
        <!-- Current Password -->
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Dabartinis slaptažodis
          </label>
          <input
            v-model="form.currentPassword"
            type="password"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            :disabled="loading"
          />
        </div>

        <!-- New Password -->
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Naujas slaptažodis
          </label>
          <input
            v-model="form.newPassword"
            type="password"
            required
            minlength="6"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            :disabled="loading"
          />
          <p v-if="isNewPasswordShort" class="text-[11px] text-red-600 mt-1"></p>
        </div>

        <!-- Confirm New Password -->
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Pakartoti naują slaptažodį
          </label>
          <input
            v-model="form.confirmPassword"
            type="password"
            required
            minlength="6"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            :disabled="loading"
          />
        </div>

        <!-- Buttons -->
        <div class="form-actions justify-end">
          <Button
            type="button"
            variant="ghost"
            @click="closeModal"
            :disabled="loading"
          >
            Atšaukti
          </Button>
          <Button
            type="submit"
            variant="primary"
            :disabled="loading"
          >
            {{ loading ? 'Keičiama...' : 'Keisti' }}
          </Button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import { usersAPI } from '@/api'
import { Button } from '@/components/common'
import { useToast } from '@/composables'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  userId: {
    type: Number,
    required: true
  }
})

const emit = defineEmits(['close', 'success'])
const { error: showError } = useToast()

const form = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const loading = ref(false)
const error = ref('')

const isNewPasswordShort = computed(() => {
  const len = form.value.newPassword.length
  return len > 0 && len < 6
})

// Reset form when modal opens
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    form.value = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    }
    error.value = ''
  }
})

const closeModal = () => {
  emit('close')
}

const handleSubmit = async () => {
  error.value = ''

  // Validate passwords match
  if (form.value.newPassword !== form.value.confirmPassword) {
    error.value = 'Naujas slaptažodis ir jo patvirtinimas nesutampa'
    return
  }

  // Validate new password is different
  if (form.value.currentPassword === form.value.newPassword) {
    error.value = 'Naujas slaptažodis turi skirtis nuo senojo'
    return
  }

  loading.value = true

  try {
    const response = await usersAPI.changePassword(props.userId, {
      currentPassword: form.value.currentPassword,
      newPassword: form.value.newPassword
    })
    
    if (response && !response.error) {
      emit('success')
      closeModal()
    } else {
      showError(response?.error?.message || 'Neteisingas dabartinis slaptažodis')
    }
  } catch (err) {
    console.error('Password change error:', err)
    showError(err.message || 'Nepavyko pakeisti slaptažodžio')
  } finally {
    loading.value = false
  }
}
</script>
