<template>
  <div class="admin-page">
    <div class="container">
      <div class="page-header">
        <h1>Administratoriaus panelis</h1>
        <span class="admin-badge">Admin</span>
      </div>

      <div class="admin-tabs">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          :class="{ active: activeTab === tab.key }"
          class="tab-button"
        >
          {{ tab.label }}
        </button>
      </div>

      <!-- Users Tab -->
      <div v-if="activeTab === 'users'" class="tab-content">
        <Card>
          <div class="section-header">
            <h2>Vartotojų valdymas</h2>
            <Button variant="primary" size="sm" @click="$router.push('/admin/users')">
              Valdyti vartotojus
            </Button>
          </div>
          
          <div class="stats-row">
            <div class="stat-item">
              <div class="stat-label">Viso vartotojų</div>
              <div class="stat-value">{{ userStats.total }}</div>
            </div>
            <div class="stat-item">
              <div class="stat-label">Administratoriai</div>
              <div class="stat-value">{{ userStats.admins }}</div>
            </div>
            <div class="stat-item">
              <div class="stat-label">Nariai</div>
              <div class="stat-value">{{ userStats.members }}</div>
            </div>
          </div>
        </Card>
      </div>

      <!-- System Tab -->
      <div v-if="activeTab === 'system'" class="tab-content">
        <Card>
          <div class="section-header">
            <h2>Sistemos informacija</h2>
          </div>
          
          <div class="system-info">
            <div class="info-row">
              <span class="info-label">Frontend versija:</span>
              <span class="info-value">1.0.0</span>
            </div>
            <div class="info-row">
              <span class="info-label">API endpoint:</span>
              <span class="info-value">{{ apiEndpoint }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Paskutinis atnaujinimas:</span>
              <span class="info-value">{{ lastUpdated }}</span>
            </div>
          </div>

          <div class="danger-zone">
            <h3>Pavojinga zona</h3>
            <p>Šie veiksmai gali pakeisti sistemos duomenis. Būkite atsargūs.</p>
            <div class="danger-actions">
              <Button variant="danger" size="sm" disabled>
                Išvalyti cache
              </Button>
              <Button variant="danger" size="sm" disabled>
                Eksportuoti duomenis
              </Button>
            </div>
          </div>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore, useActivitiesStore, useUserStore } from '@/stores'
import { useToast } from '@/composables'
import { Button, Card } from '@/components/common'
import { formatDate } from '@/utils/date'
import type { Activity } from '@/types'
import { Role } from '@/types/enums'

const router = useRouter()
const authStore = useAuthStore()
const activitiesStore = useActivitiesStore()
const userStore = useUserStore()
const { error } = useToast()

const activeTab = ref<'users' | 'activities' | 'system'>('users')

const tabs = [
  { key: 'users' as const, label: 'Vartotojai' },
  { key: 'system' as const, label: 'Sistema' }
]

// Check if user is admin
if (!authStore.isAdmin) {
  error('Neturite prieigos prie administratoriaus paneli')
  router.push('/dashboard')
}

const userStats = computed(() => {
  const users = userStore.allUsers
  return {
    total: users.length,
    admins: users.filter(u => u.role === Role.Admin).length,
    members: users.filter(u => u.role === Role.Member).length,
  }
})

const activeActivities = computed(() => {
  return activitiesStore.activities.filter((a: Activity) => 
    new Date(a.endDate || a.startDate) >= new Date()
  ).length
})

const completedActivities = computed(() => {
  return activitiesStore.activities.filter((a: Activity) => 
    new Date(a.endDate || a.startDate) < new Date()
  ).length
})

const apiEndpoint = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'
const lastUpdated = new Date().toLocaleString('lt-LT')

onMounted(async () => {
  try {
    await Promise.all([
      userStore.fetchAllUsers(),
      activitiesStore.fetchActivities()
    ])
  } catch (e) {
    error('Nepavyko gauti duomenų')
  }
})

function viewActivity(id: number) {
  router.push(`/activities/${id}`)
}
</script>

<style scoped>
.admin-page {
  padding: 2rem 1rem;
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
  min-height: calc(100vh - 200px);
}

.container {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
}

.admin-badge {
  padding: 0.375rem 0.75rem;
  background: var(--danger-light);
  color: var(--danger);
  border-radius: var(--border-radius-sm);
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.admin-tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid var(--border);
}

.tab-button {
  padding: 0.75rem 1.5rem;
  border: none;
  background: none;
  font-size: 1rem;
  font-weight: 500;
  color: var(--text-secondary);
  cursor: pointer;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  transition: var(--transition);
}

.tab-button:hover {
  color: var(--primary);
}

.tab-button.active {
  color: var(--primary);
  border-bottom-color: var(--primary);
}

.tab-content {
  animation: fadeIn 0.3s ease;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border);
}

.section-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-primary);
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-item {
  text-align: center;
  padding: 1.5rem;
  background: var(--gray-50);
  border-radius: var(--border-radius);
}

.stat-label {
  display: block;
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin-bottom: 0.5rem;
}

.stat-value {
  display: block;
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary);
}

.activities-table {
  margin-top: 2rem;
}

.activities-table h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 1rem;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: var(--gray-50);
}

th {
  padding: 0.75rem;
  text-align: left;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

td {
  padding: 1rem 0.75rem;
  border-top: 1px solid var(--border);
  font-size: 0.875rem;
  color: var(--text-primary);
}

tbody tr:hover {
  background: var(--gray-50);
}

.system-info {
  margin-bottom: 2rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid var(--border);
}

.info-label {
  font-weight: 500;
  color: var(--text-secondary);
}

.info-value {
  font-weight: 600;
  color: var(--text-primary);
}

.danger-zone {
  margin-top: 3rem;
  padding: 1.5rem;
  border: 2px solid var(--danger);
  border-radius: var(--border-radius);
  background: var(--danger-light);
}

.danger-zone h3 {
  color: var(--danger);
  margin-bottom: 0.5rem;
}

.danger-zone p {
  color: var(--text-secondary);
  margin-bottom: 1rem;
}

.danger-actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .stats-row {
    grid-template-columns: 1fr;
  }
  
  table {
    font-size: 0.75rem;
  }
  
  th, td {
    padding: 0.5rem;
  }
}
</style>
