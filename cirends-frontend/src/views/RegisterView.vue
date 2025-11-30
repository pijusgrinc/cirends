<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-header">
        <Icon name="check-circle" size="48" class="auth-icon" />
        <h1>Cirends</h1>
        <p>Sukurkite naują paskyrą</p>
      </div>

      <form @submit.prevent="handleRegister" class="auth-form">
        <div class="form-group">
          <label for="name" class="form-label required">Vardas ir pavardė</label>
          <Input
            id="name"
            v-model="name"
            type="text"
            placeholder="Jonas Petraitis"
            required
            :disabled="authStore.loading"
          />
        </div>

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
          <p class="form-hint">Mažiausiai 6 ženklai</p>
        </div>

        <div class="form-group">
          <label for="confirmPassword" class="form-label required">Patvirtinkite slaptažodį</label>
          <Input
            id="confirmPassword"
            v-model="confirmPassword"
            type="password"
            placeholder="••••••••"
            required
            :disabled="authStore.loading"
          />
        </div>

        <Button type="submit" variant="primary" :loading="authStore.loading" full-width>
          Registruotis
        </Button>
      </form>

      <div class="auth-footer">
        <p>Jau turite paskyrą? 
          <router-link to="/login" class="auth-link">Prisijunkite</router-link>
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
import { validateEmail, validatePassword } from '@/utils/validation'

const router = useRouter()
const authStore = useAuthStore()
const { success, error: showError } = useToast()

const name = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')

const handleRegister = async () => {
  // Validacija
  if (!validateEmail(email.value)) {
    showError('Neteisingas el. pašto adresas')
    return
  }

  if (!validatePassword(password.value)) {
    showError('Slaptažodis turi būti bent 6 ženklų')
    return
  }

  if (password.value !== confirmPassword.value) {
    showError('Slaptažodžiai nesutampa')
    return
  }

  try {
    const result = await authStore.register({
      email: email.value,
      password: password.value,
      name: name.value
    })
    
    if (result) {
      success('Paskyra sukurta sėkmingai!')
      router.push('/dashboard')
    } else {
      showError(authStore.error || 'Nepavyko registruotis')
    }
  } catch (error: any) {
    showError(error.message || 'Klaida registruojantis')
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
  max-width: 500px;
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

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  color: var(--dark);
  font-weight: 600;
  font-size: 0.9rem;
}

.form-label.required::after {
  content: ' *';
  color: var(--danger);
}

.form-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--gray-400);
  border-radius: 6px;
  font-size: 0.95rem;
  transition: var(--transition);
  font-family: 'Poppins', sans-serif;
}

.form-input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-input:disabled {
  background: var(--gray-100);
  cursor: not-allowed;
}

.form-hint {
  font-size: 0.8rem;
  color: var(--gray-600);
  margin-top: 0.25rem;
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

  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
