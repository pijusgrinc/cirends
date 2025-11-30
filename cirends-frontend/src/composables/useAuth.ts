import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

/**
 * Composable autentifikacijos logikai
 * Suteikia prieigą prie auth store ir pagalbines funkcijas
 * 
 * @example
 * const { isAuthenticated, user, login, logout } = useAuth()
 */
export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()

  const isAuthenticated = computed(() => authStore.isAuthenticated)
  const isAdmin = computed(() => authStore.isAdmin)
  const isMember = computed(() => authStore.isMember)
  const user = computed(() => authStore.user)
  const userName = computed(() => authStore.userName)

  async function login(email: string, password: string) {
    const success = await authStore.login({ email, password })
    
    if (success) {
      // Nukreipti į dashboard
      router.push('/dashboard')
    }
    
    return success
  }

  async function register(email: string, password: string, name: string) {
    const success = await authStore.register({ email, password, name })
    
    if (success) {
      // Nukreipti į dashboard
      router.push('/dashboard')
    }
    
    return success
  }

  async function logout() {
    await authStore.logout()
    router.push('/login')
  }

  function requireAuth() {
    if (!isAuthenticated.value) {
      router.push('/login')
      return false
    }
    return true
  }

  function requireAdmin() {
    if (!isAdmin.value) {
      router.push('/dashboard')
      return false
    }
    return true
  }

  return {
    // State
    isAuthenticated,
    isAdmin,
    isMember,
    user,
    userName,
    loading: computed(() => authStore.loading),
    error: computed(() => authStore.error),
    
    // Actions
    login,
    register,
    logout,
    requireAuth,
    requireAdmin,
    clearError: authStore.clearError
  }
}
