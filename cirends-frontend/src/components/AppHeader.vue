<template>
  <header class="app-header">
    <div class="header-content">
      <router-link to="/dashboard" class="logo">
        <Icon name="check-circle" size="32" class="logo-icon" />
        <span class="logo-text">Cirends</span>
      </router-link>

      <div class="header-actions">
        <div v-if="isLoggedIn" class="user-menu">
          <div class="user-info">
            <div class="user-avatar">{{ userInitials }}</div>
            <div class="user-details">
              <div class="user-name">{{ userName }}</div>
              <div class="user-email">{{ userEmail }}</div>
            </div>
          </div>
          <button class="logout-btn" @click="logout" title="Atsijungti">
            <Icon name="logout" />
          </button>
        </div>
        <div v-else class="auth-links">
          <router-link to="/login" class="btn btn-secondary btn-small">Prisijungti</router-link>
          <router-link to="/register" class="btn btn-primary btn-small">Registruotis</router-link>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { computed, watch } from 'vue'
import { useAuthStore } from '@/stores'
import { useRouter } from 'vue-router'
import { Icon } from '@/components/icons'

const authStore = useAuthStore()
const router = useRouter()

const isLoggedIn = computed(() => authStore.isAuthenticated)
const userName = computed(() => authStore.user?.name || 'Vartotojas')
const userEmail = computed(() => authStore.user?.email || '')

const userInitials = computed(() => {
  const parts = userName.value.split(' ')
  return parts.map(p => p[0]).join('').toUpperCase().substring(0, 2)
})

// Watch for auth changes
watch(() => authStore.isAuthenticated, async (newVal) => {
  if (newVal && !authStore.user) {
    await authStore.fetchProfile()
  }
})

const logout = async () => {
  await authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.app-header {
  background: white;
  color: var(--text-primary);
  padding: 1rem 0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  position: sticky;
  top: 0;
  z-index: 100;
  border-bottom: 1px solid var(--border-color);
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: var(--primary);
  text-decoration: none;
  transition: var(--transition);
  font-weight: 700;
  font-size: 1.5rem;
}

.logo:hover {
  transform: scale(1.02);
  color: var(--primary-dark);
}

.logo-icon {
  width: 32px;
  height: 32px;
  color: var(--primary);
}

.logo-text {
  font-family: 'Poppins', sans-serif;
  letter-spacing: 1px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 2rem;
}

.user-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.9rem;
  color: white;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.2);
}

.user-details {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--text-primary);
}

.user-email {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

.logout-btn {
  background: var(--gray-100);
  border: none;
  color: var(--text-secondary);
  width: 40px;
  height: 40px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: var(--transition);
}

.logout-btn:hover {
  background: var(--danger-light);
  color: var(--danger);
  transform: scale(1.05);
}

.logout-btn svg {
  width: 20px;
  height: 20px;
}

.auth-links {
  display: flex;
  gap: 1rem;
}

@media (max-width: 768px) {
  .header-content {
    flex-wrap: wrap;
  }

  .user-details {
    display: none;
  }

  .auth-links {
    gap: 0.5rem;
  }
}
</style>
