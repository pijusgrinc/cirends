<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-header">
        <Icon name="check-circle" size="48" class="auth-icon" />
        <h1>Cirends</h1>
        <p>Prisijunkite prie sistemos</p>
      </div>

      <form @submit.prevent="handleLogin" class="auth-form">
        <div class="form-group">
          <label for="email" class="form-label required">El. pašto adresas</label>
          <Input
            id="email"
            v-model="email"
            type="email"
            placeholder="varpav@pavyzdys.lt"
            required
            :disabled="authStore.loading"
          />
        </div>

        <div class="form-group">
          <label for="password" class="form-label required">Slaptažodis</label>
          <Input
            id="password"
            v-model="password"
            type="password"
            placeholder="••••••••"
            required
            :disabled="authStore.loading"
          />
        </div>

        <Button type="submit" variant="primary" :loading="authStore.loading" full-width>
          Prisijungti
        </Button>
      </form>

      <div class="auth-footer">
        <p>Dar neturite paskyros? 
          <router-link to="/register" class="auth-link">Registruokitės dabar</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores'
import { useToast } from '@/composables'
import { Button, Input } from '@/components/common'
import { Icon } from '@/components/icons'

const router = useRouter()
const authStore = useAuthStore()
const { success, error: showError } = useToast()

const email = ref('')
const password = ref('')

const handleLogin = async () => {
  try {
    const result = await authStore.login({ email: email.value, password: password.value })
    if (result) {
      success('Sėkmingai prisijungėte!')
      router.push('/dashboard')
    } else {
      showError(authStore.error || 'Nepavyko prisijungti')
    }
  } catch (error: any) {
    showError(error.message || 'Nepavyko prisijungti')
  }
}
</script>

<style scoped>
.auth-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
  padding: 2rem 1rem;
  animation: fadeIn 0.4s ease;
}

.auth-card {
  background: white;
  border-radius: var(--border-radius);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 420px;
  padding: 2.25rem 2rem 2rem;
  animation: slideUp 0.4s ease;
  border: 1px solid var(--gray-200);
}

.auth-header {
  text-align: center;
  margin-bottom: 2rem;
}

.auth-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 1rem;
  color: var(--primary);
}

.auth-header h1 {
  font-size: 1.9rem;
  color: var(--text-primary);
  margin: 0.5rem 0 0.25rem;
}

.auth-header p {
  color: var(--text-secondary);
  font-size: 0.9rem;
  margin: 0;
}

.auth-form {
  margin-bottom: 2rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-input:disabled {
  background: var(--gray-100);
  cursor: not-allowed;
}


.loading {
  width: 16px;
  height: 16px;
  animation: spin 1s linear infinite;
}

.auth-footer {
  text-align: center;
  padding-top: 1.5rem;
  border-top: 1px solid var(--gray-300);
}

.auth-footer p {
  margin: 0;
  color: var(--gray-600);
  font-size: 0.9rem;
}

.auth-link {
  color: var(--primary);
  text-decoration: none;
  font-weight: 600;
  transition: var(--transition);
}

.auth-link:hover {
  color: var(--primary-dark);
}

@media (max-width: 768px) {
  .auth-card {
    padding: 1.5rem;
  }

  .auth-header h1 {
    font-size: 1.5rem;
  }
}
</style>
