<template>
  <div class="profile-page">
    <div class="container">
      <div class="page-header">
        <h1>Mano profilis</h1>
      </div>

      <div v-if="loading" class="loading">Kraunama...</div>
      
      <div v-else class="profile-content">
        <Card class="profile-card">
          <div class="profile-info">
            <div class="info-row">
              <span class="label">El. paštas:</span>
              <span class="value">{{ profileData.email }}</span>
            </div>
            <div class="info-row">
              <span class="label">Vardas:</span>
              <span class="value">{{ profileData.name }}</span>
            </div>
            <div class="info-row">
              <span class="label">Rolė:</span>
              <span class="value">{{ getRoleText(profileData.role) }}</span>
            </div>
            <div class="info-row">
              <span class="label">Sukurta:</span>
              <span class="value">{{ formatDate(profileData.createdAt) }}</span>
            </div>
          </div>

          <div class="profile-actions">
            <Button variant="primary" @click="showEditModal = true">
              Redaguoti profilį
            </Button>
            <Button variant="secondary" @click="showPasswordModal = true">
              Keisti slaptažodį
            </Button>
          </div>
        </Card>
      </div>

      <!-- Edit Profile Modal -->
      <div v-if="showEditModal" class="modal-overlay" @click.self="showEditModal = false">
        <Card class="modal-card">
          <div class="modal-header">
            <h2>Redaguoti profilį</h2>
            <button class="close-btn" @click="showEditModal = false">×</button>
          </div>
          
          <form @submit.prevent="handleUpdate" class="edit-form">
            <div class="form-group">
              <label for="name" class="form-label">Vardas</label>
              <Input
                id="name"
                v-model="editForm.name"
                type="text"
                placeholder="Vardas ir pavardė"
                required
              />
            </div>

            <div class="form-group">
              <label for="email" class="form-label">El. paštas</label>
              <Input
                id="email"
                v-model="editForm.email"
                type="email"
                placeholder="varpav@pavyzdys.lt"
                required
              />
            </div>

            <div class="form-actions">
              <Button type="submit" variant="primary" :loading="saving" full-width>
                Išsaugoti
              </Button>
              <Button type="button" variant="secondary" @click="showEditModal = false">
                Atšaukti
              </Button>
            </div>
          </form>
        </Card>
      </div>

      <!-- Change Password Modal -->
      <ChangePasswordModal 
        :is-open="showPasswordModal"
        :user-id="profileData.id"
        @close="showPasswordModal = false"
        @success="handlePasswordChanged"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores'
import { usersAPI } from '@/api'
import { Button, Card, Input } from '@/components/common'
import ChangePasswordModal from '@/components/ChangePasswordModal.vue'
import { useToast } from '@/composables'
import { Role, getRoleLabel } from '@/types/enums'
import type { User } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const { success, error } = useToast()

const loading = ref(true)
const saving = ref(false)
const showEditModal = ref(false)
const showPasswordModal = ref(false)

const profileData = ref<User>({
  id: 0,
  name: '',
  email: '',
  role: Role.Member,
  createdAt: ''
})

const editForm = ref({
  name: '',
  email: ''
})

onMounted(async () => {
  await loadProfile()
})

async function loadProfile() {
  loading.value = true
  try {
    const response = await usersAPI.getProfile()
    if (response.ok && response.data) {
      profileData.value = response.data
      editForm.value.name = response.data.name
      editForm.value.email = response.data.email
    } else {
      error('Nepavyko įkelti profilio')
    }
  } catch (e) {
    error('Klaida įkeliant profilį')
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function handleUpdate() {
  saving.value = true
  try {
    const response = await usersAPI.updateUser(profileData.value.id, editForm.value)
    if (response.ok && response.data) {
      profileData.value = response.data
      // Update auth store
      await authStore.fetchCurrentUser()
      success('Profilis atnaujintas')
      showEditModal.value = false
    } else {
      error(response.error?.message || 'Nepavyko atnaujinti profilio')
    }
  } catch (e) {
    error('Klaida atnaujinant profilį')
    console.error(e)
  } finally {
    saving.value = false
  }
}

function handlePasswordChanged() {
  success('Slaptažodis sėkmingai pakeistas')
}

function formatDate(dateStr: string) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('lt-LT', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

function getRoleText(role: Role | string) {
  if (typeof role === 'number') return getRoleLabel(role)
  switch (role) {
    case 'Admin': return 'Administratorius'
    case 'User': return 'Naudotojas'
    case 'Guest': return 'Svečias'
    default: return role
  }
}
</script>

<style scoped>
.profile-page {
  padding: 2rem 1rem;
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
  min-height: calc(100vh - 200px);
}

.container {
  max-width: 800px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
}

.loading {
  text-align: center;
  padding: 2rem;
  color: var(--text-secondary);
}

.profile-card {
  background: white;
}

.profile-info {
  margin-bottom: 2rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid var(--border);
}

.info-row:last-child {
  border-bottom: none;
}

.label {
  font-weight: 500;
  color: var(--text-secondary);
}

.value {
  font-weight: 600;
  color: var(--text-primary);
}

.profile-actions {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  border-top: 1px solid var(--border);
}

.edit-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group:last-of-type {
  margin-bottom: 0;
}

.edit-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group:last-of-type {
  margin-bottom: 0;
}

.form-input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-light);
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border);
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.75);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.modal-card {
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  animation: slideUp 0.3s ease;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.2), 0 10px 10px -5px rgba(0, 0, 0, 0.1);
  background: white;
}

@keyframes slideUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid var(--border);
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-primary);
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.75rem;
  cursor: pointer;
  color: var(--text-secondary);
  padding: 0;
  width: 2.5rem;
  height: 2.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: var(--transition);
}

.close-btn:hover {
  background: var(--gray-100);
  color: var(--text-primary);
}

.edit-form {
  padding: 1rem 1.5rem 1.5rem;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  font-size: 0.9375rem;
  color: var(--text-primary);
}

.form-actions {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--border);
}

@media (max-width: 768px) {
  .page-header h1 {
    font-size: 1.5rem;
  }

  .info-row {
    flex-direction: column;
    gap: 0.5rem;
  }

  .profile-actions {
    flex-direction: column;
  }

  .modal-card {
    margin: 0;
  }

  .modal-header h2 {
    font-size: 1.25rem;
  }
}
</style>
