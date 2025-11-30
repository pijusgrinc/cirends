import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { usersAPI } from '@/api'
import type { User, UpdateUserRequest } from '@/types'
import { Role } from '@/types/enums'

/**
 * Vartotojo profilio store
 */
export const useUserStore = defineStore('user', () => {
  // State
  const users = ref<User[]>([])
  const currentUser = ref<User | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const allUsers = computed(() => users.value)
  const usersCount = computed(() => users.value.length)

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

  // Actions
  async function fetchAllUsers() {
    loading.value = true
    error.value = null
    
    try {
      const response = await usersAPI.getAllUsers()
      
      if (response.ok && response.data) {
        const arr = Array.isArray(response.data) ? response.data : []
        users.value = arr.map((u: any) => ({
          id: u.id,
          name: u.name,
          email: u.email,
          role: normalizeRole(u.role),
          createdAt: u.createdAt
        }))
        return users.value
      } else {
        error.value = response.error?.message || 'Nepavyko gauti vartotojų'
        users.value = []
        return []
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant vartotojus'
      console.error('Fetch users error:', e)
      users.value = []
      return []
    } finally {
      loading.value = false
    }
  }

  async function fetchUser(userId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await usersAPI.getUserById(userId)
      
      if (response.ok && response.data) {
        const u: any = response.data
        currentUser.value = {
          id: u.id,
          name: u.name,
          email: u.email,
          role: normalizeRole(u.role),
          createdAt: u.createdAt
        }
        return currentUser.value
      } else {
        error.value = response.error?.message || 'Vartotojas nerastas'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant vartotoją'
      console.error('Fetch user error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateProfile(data: UpdateUserRequest) {
    loading.value = true
    error.value = null
    
    try {
      const response = await usersAPI.updateProfile(data)
      
      if (response.ok && response.data) {
        const u: any = response.data
        currentUser.value = {
          id: u.id,
          name: u.name,
          email: u.email,
          role: normalizeRole(u.role),
          createdAt: u.createdAt
        }
        return currentUser.value
      } else {
        error.value = response.error?.message || 'Nepavyko atnaujinti profilio'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida atnaujinant profilį'
      console.error('Update profile error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function deleteUser(userId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await usersAPI.deleteUser(userId)
      
      if (response.ok) {
        // Pašalinti iš sąrašo
        users.value = users.value.filter(u => u.id !== userId)
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko ištrinti vartotojo'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida trinant vartotoją'
      console.error('Delete user error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  return {
    // State
    users,
    currentUser,
    loading,
    error,
    
    // Getters
    allUsers,
    usersCount,
    
    // Actions
    fetchAllUsers,
    fetchUser,
    updateProfile,
    deleteUser,
    clearError
  }
})
