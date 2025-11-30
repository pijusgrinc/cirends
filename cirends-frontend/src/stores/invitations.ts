import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { invitationsAPI } from '@/api'
import type { Invitation, CreateInvitationRequest } from '@/types'
import { InvitationStatus } from '@/types'

/**
 * Kvietimų store
 * Valdo kvietimus į veiklas
 */
export const useInvitationsStore = defineStore('invitations', () => {
  // State
  const invitations = ref<Invitation[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const pendingInvitations = computed(() => {
    return invitations.value.filter(i => i.status === InvitationStatus.Pending)
  })

  const acceptedInvitations = computed(() => {
    return invitations.value.filter(i => i.status === InvitationStatus.Accepted)
  })

  const rejectedInvitations = computed(() => {
    return invitations.value.filter(i => i.status === InvitationStatus.Rejected)
  })

  const pendingCount = computed(() => pendingInvitations.value.length)

  // Actions
  async function fetchInvitations(forceRefresh = false) {
    if (invitations.value.length > 0 && !forceRefresh) {
      return invitations.value
    }

    loading.value = true
    error.value = null
    
    try {
      const response = await invitationsAPI.getAll()
      
      if (response.ok && response.data) {
        invitations.value = response.data
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko gauti kvietimų'
        return []
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant kvietimus'
      console.error('Fetch invitations error:', e)
      return []
    } finally {
      loading.value = false
    }
  }

  async function fetchPendingInvitations() {
    loading.value = true
    error.value = null
    
    try {
      const response = await invitationsAPI.getPending()
      
      if (response.ok && response.data) {
        // Atnaujinti pending kvietimus
        invitations.value = [
          ...invitations.value.filter(i => i.status !== InvitationStatus.Pending),
          ...response.data
        ]
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko gauti laukiančių kvietimų'
        return []
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant laukiančius kvietimus'
      console.error('Fetch pending invitations error:', e)
      return []
    } finally {
      loading.value = false
    }
  }

  async function sendInvitation(activityId: number, userId: number) {
    loading.value = true
    error.value = null
    
    try {
      // Note: userId should be email string in the API
      const response = await invitationsAPI.send(userId.toString(), activityId)
      
      if (response.ok && response.data) {
        invitations.value.push(response.data)
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko išsiųsti kvietimo'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida siunčiant kvietimą'
      console.error('Send invitation error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function acceptInvitation(invitationId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await invitationsAPI.accept(invitationId)
      
      if (response.ok) {
        // Atnaujinti kvietimo būseną
        const invitation = invitations.value.find(i => i.id === invitationId)
        if (invitation) {
          invitation.status = InvitationStatus.Accepted
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko priimti kvietimo'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida priimant kvietimą'
      console.error('Accept invitation error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function rejectInvitation(invitationId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await invitationsAPI.reject(invitationId)
      
      if (response.ok) {
        // Atnaujinti kvietimo būseną
        const invitation = invitations.value.find(i => i.id === invitationId)
        if (invitation) {
          invitation.status = InvitationStatus.Rejected
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko atmesti kvietimo'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida atmetant kvietimą'
      console.error('Reject invitation error:', e)
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
    invitations,
    loading,
    error,
    
    // Getters
    pendingInvitations,
    acceptedInvitations,
    rejectedInvitations,
    pendingCount,
    
    // Actions
    fetchInvitations,
    fetchPendingInvitations,
    sendInvitation,
    acceptInvitation,
    rejectInvitation,
    clearError
  }
})
