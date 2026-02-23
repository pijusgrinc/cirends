// API konfiguracija
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'

// Header helper
const getHeaders = (isJson = true, customHeaders = {}) => {
  const headers = { ...customHeaders }
  if (isJson && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json'
  }
  return headers
}

async function apiCall(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`
  const headers = getHeaders(options.method !== 'GET' && !options.formData, options.headers)
  
  const config = {
    method: options.method || 'GET',
    headers,
    credentials: 'include' // CORS credentials
  }
  
  if (options.formData) {
    delete config.headers['Content-Type']
    config.body = options.formData
  } else if (options.body && typeof options.body === 'object') {
    config.body = JSON.stringify(options.body)
  }
  
  try {
    const response = await fetch(url, config)
    
    let data = null
    const contentType = response.headers.get('content-type')
    
    if (contentType && contentType.includes('application/json')) {
      data = await response.json()
    } else {
      data = await response.text()
    }
    
    // Handle 401 Unauthorized - session expired or token invalid
    if (response.status === 401) {
      console.warn('Unauthorized (401) - session expired or token invalid')
      // Auto-logout on 401
      try {
        const { useAuthStore } = await import('@/stores/auth')
        const authStore = useAuthStore()
        if (authStore.isAuthenticated) {
          await authStore.logout()
          // Redirect to login
          const router = (await import('@/router')).default
          router.push('/login')
        }
      } catch (err) {
        console.error('Failed to handle 401:', err)
      }
    }
    
    return {
      ok: response.ok,
      status: response.status,
      data,
      error: !response.ok ? data : null
    }
  } catch (error) {
    console.error('API Error:', error)
    return {
      ok: false,
      status: 0,
      data: null,
      error: error.message
    }
  }
}

// ============== AUTENTIFIKACIJA ==============

export const authAPI = {
  async register(email, password, name) {
    return apiCall('/auth/register', {
      method: 'POST',
      body: {
        email,
        password,
        name
      }
    })
  },
  
  async login(email, password) {
    return apiCall('/auth/login', {
      method: 'POST',
      body: { email, password }
    })
  },
  
  async logout() {
    return apiCall('/auth/logout', { method: 'POST' })
  },
  
  async refresh() {
    return apiCall('/auth/refresh', { method: 'POST' })
  }
}

// ============== Naudotojai ==============

export const usersAPI = {
  async getProfile() {
    return apiCall('/users/profile')
  },
  
  async getAllUsers() {
    return apiCall('/users')
  },
  
  async getUserById(id) {
    return apiCall(`/users/${id}`)
  },
  
  async updateUser(id, data) {
    return apiCall(`/users/${id}`, {
      method: 'PUT',
      body: data
    })
  },

  async changePassword(id, currentPassword, newPassword) {
    return apiCall(`/users/${id}/password`, {
      method: 'PUT',
      body: { currentPassword, newPassword }
    })
  },
  
  async deleteUser(id) {
    return apiCall(`/users/${id}`, {
      method: 'DELETE'
    })
  },

  // Admin endpoints
  async updateUserRole(userId, role) {
    return apiCall(`/users/${userId}/role`, {
      method: 'PUT',
      body: { role }
    })
  },

  async toggleUserActive(userId) {
    return apiCall(`/users/${userId}/toggle-active`, {
      method: 'PUT'
    })
  },

  async getStatistics() {
    return apiCall('/users/statistics')
  },

  async getAllActivities() {
    return apiCall('/users/activities')
  }
}

// ============== VEIKLOS ==============

export const activitiesAPI = {
  async getAll() {
    return apiCall('/activities')
  },
  
  async getById(id) {
    return apiCall(`/activities/${id}`)
  },
  
  async create(data) {
    return apiCall('/activities', {
      method: 'POST',
      body: data
    })
  },
  
  async update(id, data) {
    return apiCall(`/activities/${id}`, {
      method: 'PUT',
      body: data
    })
  },
  
  async delete(id) {
    return apiCall(`/activities/${id}`, {
      method: 'DELETE'
    })
  },
  
  async getParticipants(id) {
    return apiCall(`/activities/${id}/participants`)
  },
  
  async addParticipant(id, userId) {
    return apiCall(`/activities/${id}/participants`, {
      method: 'POST',
      body: { userId }
    })
  },
  
  async removeParticipant(id, userId) {
    return apiCall(`/activities/${id}/participants/${userId}`, {
      method: 'DELETE'
    })
  }
}

// ============== UŽDUOTYS ==============

export const tasksAPI = {
  async getAll(activityId) {
    return apiCall(`/activities/${activityId}/tasks`)
  },
  
  async getById(activityId, taskId) {
    return apiCall(`/activities/${activityId}/tasks/${taskId}`)
  },
  
  async create(activityId, data) {
    return apiCall(`/activities/${activityId}/tasks`, {
      method: 'POST',
      body: data
    })
  },
  
  async update(activityId, taskId, data) {
    return apiCall(`/activities/${activityId}/tasks/${taskId}`, {
      method: 'PUT',
      body: data
    })
  },
  
  async delete(activityId, taskId) {
    return apiCall(`/activities/${activityId}/tasks/${taskId}`, {
      method: 'DELETE'
    })
  }
}

// ============== IŠLAIDOS ==============

export const expensesAPI = {
  async getAll(activityId) {
    return apiCall(`/activities/${activityId}/expenses`)
  },
  
  async getById(activityId, expenseId) {
    return apiCall(`/activities/${activityId}/expenses/${expenseId}`)
  },
  
  async create(activityId, data) {
    return apiCall(`/activities/${activityId}/expenses`, {
      method: 'POST',
      body: data
    })
  },
  
  async update(activityId, expenseId, data) {
    return apiCall(`/activities/${activityId}/expenses/${expenseId}`, {
      method: 'PUT',
      body: data
    })
  },
  
  async delete(activityId, expenseId) {
    return apiCall(`/activities/${activityId}/expenses/${expenseId}`, {
      method: 'DELETE'
    })
  },

  async markShareAsPaid(activityId, expenseId, shareId) {
    return apiCall(`/activities/${activityId}/expenses/${expenseId}/shares/${shareId}/mark-paid`, {
      method: 'PATCH'
    })
  },

  async unmarkShareAsPaid(activityId, expenseId, shareId) {
    return apiCall(`/activities/${activityId}/expenses/${expenseId}/shares/${shareId}/unmark-paid`, {
      method: 'PATCH'
    })
  },

  async markAllExpensesAsPaidForActivity(activityId) {
    return apiCall(`/activities/${activityId}/expenses/mark-all-paid`, {
      method: 'POST'
    })
  }
}

// ============== KVIETIMAI ==============

export const invitationsAPI = {
  async getAll() {
    return apiCall('/invitations')
  },
  
  async getPending() {
    return apiCall('/invitations/pending')
  },
  
  async getActivityInvitations(activityId) {
    return apiCall(`/invitations/activity/${activityId}`)
  },
  
  async send(email, activityId) {
    return apiCall('/invitations', {
      method: 'POST',
      body: { email, activityId }
    })
  },
  
  async accept(invitationId) {
    return apiCall(`/invitations/${invitationId}/accept`, {
      method: 'POST'
    })
  },
  
  async reject(invitationId) {
    return apiCall(`/invitations/${invitationId}/reject`, {
      method: 'POST'
    })
  }
}

// ============== ADMIN ==============

export default {
  auth: authAPI,
  users: usersAPI,
  activities: activitiesAPI,
  tasks: tasksAPI,
  expenses: expensesAPI,
  invitations: invitationsAPI
}
