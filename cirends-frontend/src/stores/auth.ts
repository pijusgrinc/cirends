import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI, setToken, removeToken, getToken } from '@/api'
import type { User, LoginRequest, RegisterRequest } from '@/types'
import { Role } from '@/types/enums'

/**
 * Autentifikacijos store
 * Valdo vartotojo prisijungimą, registraciją ir sesijos būseną
 */
export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(getToken())
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  // After normalization, role is always numeric Role enum
  const isAdmin = computed(() => user.value?.role === Role.Admin)
  const isMember = computed(() => user.value?.role === Role.Member)
  const userName = computed(() => user.value?.name || 'Vartotojas')

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
        token.value = response.data.token
        user.value = normalizeUser(response.data.user)
        setToken(response.data.token)
        
        if (response.data.refreshToken) {
          localStorage.setItem('refreshToken', response.data.refreshToken)
        }
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
        token.value = response.data.token
        user.value = normalizeUser(response.data.user)
        setToken(response.data.token)
        
        if (response.data.refreshToken) {
          localStorage.setItem('refreshToken', response.data.refreshToken)
        }
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
      removeToken()
      localStorage.removeItem('refreshToken')
      // Clear other stores to avoid leaking previous user's state
      try {
        const { useActivitiesStore } = await import('./activities')
        const { useTasksStore } = await import('./tasks')
        const { useExpensesStore } = await import('./expenses')
        const { useUserStore } = await import('./user')
        useActivitiesStore().reset()
        useTasksStore().reset()
        useExpensesStore().reset()
        useUserStore().$reset?.()
      } catch (err) {
        console.warn('Store reset on logout failed:', err)
      }
      loading.value = false
    }
  }

  async function fetchCurrentUser() {
    if (!token.value) return false
    
    loading.value = true
    error.value = null
    
    try {
      const { usersAPI } = await import('@/api')
      const response = await usersAPI.getProfile()
      
      if (response.ok && response.data) {
        user.value = normalizeUser(response.data)
        return true
      } else {
        // Token galimai nebegalioja
        await logout()
        return false
      }
    } catch (e) {
      console.error('Fetch user error:', e)
      await logout()
      return false
    } finally {
      loading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  // Initialize - jei yra token, pabandome gauti vartotojo duomenis
  if (token.value && !user.value) {
    fetchCurrentUser()
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
    
    // Actions
    login,
    register,
    logout,
    fetchCurrentUser,
    clearError
  }
})
