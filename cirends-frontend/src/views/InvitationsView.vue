<template>
  <div class="invitations-page">
    <div class="container">
      <div class="page-header">
        <h1>Mano kvietimai</h1>
        <p class="subtitle">Pakvietimai į veiklas, kuriuos gavote</p>
      </div>

      <Loading v-if="loading" />

      <div v-else-if="invitations.length > 0" class="invitations-list">
        <Card v-for="invitation in invitations" :key="invitation.id" class="invitation-card">
          <div class="invitation-header">
            <div class="invitation-info">
              <h3>{{ invitation.activityName || invitation.activity?.name || 'Veikla' }}</h3>
              <p class="invitation-from">Pakvietė: {{ invitation.invitedBy?.name || 'Nežinomas' }}</p>
              <p class="invitation-date">{{ formatDate(invitation.createdAt) }}</p>
            </div>
            <span :class="['invitation-status', `status-${invitation.status}`]">
              {{ getStatusLabel(invitation.status) }}
            </span>
          </div>

          <div v-if="invitation.status === 0" class="invitation-actions">
            <Button 
              variant="primary" 
              @click="handleAccept(invitation.id)"
              :disabled="actionLoading"
            >
              Priimti
            </Button>
            <Button 
              variant="ghost" 
              @click="handleReject(invitation.id)"
              :disabled="actionLoading"
            >
              Atmesti
            </Button>
          </div>
        </Card>
      </div>

      <EmptyState 
        v-else 
        title="Neturite kvietimų" 
        description="Kai kas nors pakvies jus į veiklą, pakvietimas atsiras čia"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { invitationsAPI } from '@/api'
import { useToast } from '@/composables'
import { formatDate } from '@/utils/date'
import { Button, Loading, EmptyState, Card } from '@/components/common'
import type { Invitation } from '@/types'

const router = useRouter()
const { success, error } = useToast()

const invitations = ref<Invitation[]>([])
const loading = ref(false)
const actionLoading = ref(false)

async function loadInvitations() {
  loading.value = true
  try {
    const response = await invitationsAPI.getPending()
    if (response.ok && response.data) {
      invitations.value = response.data
    } else {
      error('Nepavyko gauti kvietimų')
    }
  } catch (e) {
    error('Netikėta klaida gaunant kvietimus')
    console.error('Load invitations error:', e)
  } finally {
    loading.value = false
  }
}

async function handleAccept(invitationId: number) {
  actionLoading.value = true
  try {
    const response = await invitationsAPI.accept(invitationId)
    if (response.ok) {
      success('Pakvietimas priimtas!')
      await loadInvitations()
    } else {
      error(response.error?.message || 'Nepavyko priimti pakvietimo')
    }
  } catch (e) {
    error('Netikėta klaida priimant pakvietimą')
    console.error('Accept invitation error:', e)
  } finally {
    actionLoading.value = false
  }
}

async function handleReject(invitationId: number) {
  actionLoading.value = true
  try {
    const response = await invitationsAPI.reject(invitationId)
    if (response.ok) {
      success('Pakvietimas atmestas')
      await loadInvitations()
    } else {
      error(response.error?.message || 'Nepavyko atmesti pakvietimo')
    }
  } catch (e) {
    error('Netikėta klaida atmestant pakvietimą')
    console.error('Reject invitation error:', e)
  } finally {
    actionLoading.value = false
  }
}

function getStatusLabel(status: number): string {
  const labels: Record<number, string> = {
    0: 'Laukiama',
    1: 'Priimta',
    2: 'Atmesta'
  }
  return labels[status] || 'Nežinoma'
}

onMounted(() => {
  loadInvitations()
})
</script>

<style scoped>
.invitations-page {
  padding: 2rem 1rem;
  min-height: 100vh;
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
}

.container {
  max-width: 900px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.subtitle {
  font-size: 1rem;
  color: var(--text-secondary);
}

.invitations-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.invitation-card {
  padding: 1.5rem;
}

.invitation-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}

.invitation-info {
  flex: 1;
}

.invitation-info h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 0.5rem;
}

.invitation-from {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin-bottom: 0.25rem;
}

.invitation-date {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0;
}

.invitation-status {
  padding: 0.375rem 0.75rem;
  border-radius: var(--border-radius-sm);
  font-size: 0.875rem;
  font-weight: 600;
  white-space: nowrap;
}

.invitation-status.status-0 { 
  background: var(--warning-light); 
  color: var(--warning); 
}

.invitation-status.status-1 { 
  background: var(--success-light); 
  color: var(--success); 
}

.invitation-status.status-2 { 
  background: var(--gray-100); 
  color: var(--text-secondary); 
}

.invitation-actions {
  display: flex;
  gap: 0.75rem;
}

@media (max-width: 768px) {
  .invitation-header {
    flex-direction: column;
  }
  
  .invitation-actions {
    flex-direction: column;
  }
}
</style>
