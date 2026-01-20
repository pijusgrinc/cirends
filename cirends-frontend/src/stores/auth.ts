import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI } from '@/api'
import type { User, LoginRequest, RegisterRequest } from '@/types'
import { Role } from '@/types/enums'

/**
 * Autentifikacijos store
 * Valdo naudotojo prisijungimą, registraciją ir sesijos būseną
 */
export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const hydrated = ref(false)

  // Getters
  const isAuthenticated = computed(() => !!user.value)
  // After normalization, role is always numeric Role enum
  const isAdmin = computed(() => user.value?.role === Role.Admin)
  const isMember = computed(() => user.value?.role === Role.Member)
  const userName = computed(() => user.value?.name || 'Naudotojas')

  // Helpers
  function normalizeRole(role: User['role']): Role {
    if (typeof role === 'number') return role as Role
    switch (role) {
      case 'Admin':
        return Role.Admin
      case 'User':
      case 'Member':
        return Role.Member
      case 'Guest':
      default:
        return Role.Guest
    }
  }

  function normalizeUser(u: any): User {
    return {
      id: u.id,
      name: u.name,
      email: u.email,
      role: normalizeRole(u.role),
      createdAt: u.createdAt
    }
  }

  // Actions
  async function login(credentials: LoginRequest) {
    loading.value = true
    error.value = null
    
    try {
      const response = await authAPI.login(credentials.email, credentials.password)
      
      if (response.ok && response.data) {
        token.value = response.data.token || null
        user.value = normalizeUser(response.data.user)
        try {
          const { useActivitiesStore } = await import('./activities')
          await useActivitiesStore().fetchActivities(true)
        } catch (postInitErr) {
          console.warn('Post-login init failed:', postInitErr)
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Prisijungti nepavyko'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida prisijungiant'
      console.error('Login error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function register(data: RegisterRequest) {
    loading.value = true
    error.value = null
    
    try {
      const response = await authAPI.register(data.email, data.password, data.name)
      
      if (response.ok && response.data) {
        token.value = response.data.token || null
        user.value = normalizeUser(response.data.user)
        // Post-register: refresh activity data to avoid stale views
        try {
          const { useActivitiesStore } = await import('./activities')
          await useActivitiesStore().fetchActivities(true)
        } catch (postInitErr) {
          console.warn('Post-register init failed:', postInitErr)
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Registracija nepavyko'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida registruojantis'
      console.error('Register error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    loading.value = true
    
    try {
      await authAPI.logout()
    } catch (e) {
      console.error('Logout error:', e)
    } finally {
      token.value = null
      user.value = null
      // Clear other stores to avoid leaking previous user's state
      try {
        const { useActivitiesStore } = await import('./activities')
        const { useTasksStore } = await import('./tasks')
        const { useExpensesStore } = await import('./expenses')
        const { useUserStore } = await import('./user')
        useActivitiesStore().reset()
        useTasksStore().reset()
        useExpensesStore().reset()
        useUserStore().reset?.()
      } catch (err) {
        console.warn('Store reset on logout failed:', err)
      }
      loading.value = false
    }
  }

  async function fetchCurrentUser() {
    loading.value = true
    error.value = null
    const isInitializing = !user.value
    
    try {
      const { usersAPI } = await import('@/api')
      const response = await usersAPI.getProfile()
      
      if (response.ok && response.data) {
        user.value = normalizeUser(response.data)
        return true
      } else {
        // Token galimai nebegalioja arba naudotojas neprisijungęs
        if (response.status === 401) {
          return false
        }
        // Only logout if we had a user before (session expired, not initial load)
        if (!isInitializing) {
          await logout()
        }
        return false
      }
    } catch (e) {
      console.error('Fetch user error:', e)
      // On initial load, just fail silently; on refresh, logout only if user was loaded
      if (!isInitializing) {
        await logout()
      }
      return false
    } finally {
      loading.value = false
      hydrated.value = true
    }
  }

  function clearError() {
    error.value = null
  }

  // Initialize - pabandome gauti naudotojo duomenis pagal esamas slapukų sesijas
  let isInitializing = false
  if (!user.value && !isInitializing) {
    isInitializing = true
    fetchCurrentUser().finally(() => {
      isInitializing = false
    })
  }

  // Auto-refresh token every 50 minutes (before 60min expiry)
  // This ensures token stays valid and user isn't kicked out mid-session
  let refreshInterval: ReturnType<typeof setInterval> | undefined
  function startAutoRefresh() {
    refreshInterval = setInterval(async () => {
      if (isAuthenticated.value && user.value) {
        try {
          const { authAPI } = await import('@/api')
          await (authAPI as any).refresh()
        } catch (e) {
          console.warn('Auto-refresh failed:', e)
        }
      }
    }, 50 * 60 * 1000) // Every 50 minutes
  }

  if (typeof window !== 'undefined') {
    startAutoRefresh()
  }

  return {
    // State
    user,
    token,
    loading,
    error,
    
    // Getters
    isAuthenticated,
    isAdmin,
    isMember,
    userName,
    hydrated,
    
    // Actions
    login,
    register,
    logout,
    fetchCurrentUser,
    clearError
  }
})
