import { ref } from 'vue'
import { defineStore } from 'pinia'
// @ts-ignore
import api from '@/api.js'

export interface SystemStatistics {
  totalUsers: number
  activeUsers: number
  totalActivities: number
  totalTasks: number
  totalExpenses: number
  completedTasks: number
  totalExpenseAmount: number
}

export interface AdminUser {
  id: number
  name: string
  email: string
  role: string
  isActive: boolean
  createdAt: string
}

export const useAdminStore = defineStore('admin', () => {
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  const users = ref<AdminUser[]>([])
  const statistics = ref<SystemStatistics | null>(null)

  async function fetchAllUsers() {
    loading.value = true
    error.value = null
    
    try {
      // @ts-ignore
      const resp = await api.users.getAllUsers()
      if (resp && resp.ok) {
        users.value = Array.isArray(resp.data) ? resp.data : []
        return users.value
      } else {
        users.value = []
        error.value = (resp && resp.error && resp.error.message) || 'Failed to fetch users'
        return []
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch users'
      console.error('Fetch users error:', e)
      return []
    } finally {
      loading.value = false
    }
  }

  async function updateUserRole(userId: number, role: string) {
    loading.value = true
    error.value = null
    
    try {
      // @ts-ignore
      await api.users.updateUserRole(userId, role)
      // Update local cache
      const user = users.value.find(u => u.id === userId)
      if (user) {
        user.role = role
      }
      return true
    } catch (e: any) {
      error.value = e.message || 'Failed to update user role'
      console.error('Update role error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function toggleUserActive(userId: number) {
    loading.value = true
    error.value = null
    
    try {
      // @ts-ignore
      await api.users.toggleUserActive(userId)
      // Update local cache
      const user = users.value.find(u => u.id === userId)
      if (user) {
        user.isActive = !user.isActive
      }
      return true
    } catch (e: any) {
      error.value = e.message || 'Failed to toggle user status'
      console.error('Toggle active error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function deleteUser(userId: number) {
    loading.value = true
    error.value = null
    
    try {
      // @ts-ignore
      await api.users.deleteUser(userId)
      // Remove from local cache
      users.value = users.value.filter(u => u.id !== userId)
      return true
    } catch (e: any) {
      error.value = e.message || 'Failed to delete user'
      console.error('Delete user error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function fetchStatistics() {
    loading.value = true
    error.value = null
    
    try {
      // @ts-ignore
      const data = await api.users.getStatistics()
      statistics.value = data
      return data
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch statistics'
      console.error('Fetch statistics error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    users,
    statistics,
    fetchAllUsers,
    updateUserRole,
    toggleUserActive,
    deleteUser,
    fetchStatistics
  }
})
