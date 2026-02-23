<template>
  <div class="dashboard">
    <div class="container">
      <!-- Welcome Section -->
      <section class="welcome-section">
        <div class="welcome-content">
          <h1>Sveiki, {{ authStore.userName }}!</h1>
          <p>Jūs turite {{ stats.activitiesCount === 1 ? `${stats.activitiesCount} aktyvią veiklą` : `${stats.activitiesCount} aktyvias veiklas` }} ir {{ stats.pendingInvitations === 1 ? `${stats.pendingInvitations} kvietimą` : `${stats.pendingInvitations} kvietimų` }}</p>
        </div>
        <div class="welcome-actions">
          <Button variant="primary" @click="$router.push('/activities')">
            <template #icon>
              <Icon name="activities" />
            </template>
            Visos veiklos
          </Button>
          <Button variant="success" @click="showActivityModal = true">
            <template #icon>
              <Icon name="add" />
            </template>
            Nauja veikla
          </Button>
        </div>
      </section>

      <!-- Stats Grid -->
      <section class="stats-grid">
        <Card class="stat-card">
          <div class="stat-icon primary">
            <Icon name="tasks" />
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.activitiesCount }}</div>
            <div class="stat-label">{{ stats.activitiesCount === 1 ? 'Veikla' : 'Veiklos' }}</div>
          </div>
        </Card>

        <Card class="stat-card">
          <div class="stat-icon success">
            <Icon name="check" />
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.tasksCount }}</div>
            <div class="stat-label">Užduotys</div>
          </div>
        </Card>

        <Card class="stat-card">
          <div class="stat-icon warning">
            <Icon name="expenses" />
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.expensesCount }}</div>
            <div class="stat-label">Išlaidos</div>
          </div>
        </Card>

        <Card class="stat-card">
          <div class="stat-icon danger">
            <Icon name="invitations" />
          </div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.pendingInvitations }}</div>
            <div class="stat-label">Kvietimai</div>
          </div>
        </Card>
      </section>

      <!-- Recent Activities -->
      <section class="recent-section">
        <h2>Naujausios veiklos</h2>
        <div v-if="recentActivities.length > 0" class="activities-list">
          <ActivityCard
            v-for="activity in recentActivities"
            :key="activity.id"
            :activity="activity"
            @click="$router.push(`/activities/${activity.id}`)"
          />
        </div>
        <EmptyState
          v-else
          title="Nėra veiklų"
          description="Sukurkite pirmąją!"
        >
          <Button variant="primary" @click="showActivityModal = true">
            Sukurti veiklą
          </Button>
        </EmptyState>
      </section>
    </div>

    <!-- Activity Modal -->
    <Modal v-if="showActivityModal" @close="showActivityModal = false">
      <template #header>Sukurti naują veiklą</template>
      <template #body>
        <ActivityForm
          :loading="activitiesStore.loading"
          @submit="handleCreateActivity"
          @cancel="showActivityModal = false"
        />
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore, useActivitiesStore, useTasksStore, useExpensesStore, useInvitationsStore } from '@/stores'
import { useToast } from '@/composables'
import { Button, Card, EmptyState } from '@/components/common'
import { ActivityCard, ActivityForm } from '@/components/activities'
import { Icon } from '@/components/icons'
import Modal from '@/components/Modal.vue'
import type { CreateActivityRequest } from '@/types'

const authStore = useAuthStore()
const activitiesStore = useActivitiesStore()
const tasksStore = useTasksStore()
const expensesStore = useExpensesStore()
const invitationsStore = useInvitationsStore()
const { success, error } = useToast()

const showActivityModal = ref(false)

const stats = computed(() => ({
  activitiesCount: activitiesStore.activities.length,
  tasksCount: Object.values(tasksStore.tasksByActivity).flat().length,
  expensesCount: Object.values(expensesStore.expensesByActivity).flat().length,
  pendingInvitations: invitationsStore.pendingInvitations.length
}))

const recentActivities = computed(() => activitiesStore.sortedActivities.slice(0, 5))

onMounted(async () => {
  try {
    await Promise.all([
      activitiesStore.fetchActivities(),
      invitationsStore.fetchPendingInvitations()
    ])
  } catch (e) {
    error('Nepavyko gauti duomenų')
  }
})

async function handleCreateActivity(data: CreateActivityRequest) {
  try {
    await activitiesStore.createActivity(data)
    success('Veikla sukurta!')
    showActivityModal.value = false
  } catch (e) {
    error('Nepavyko sukurti veiklos')
  }
}
</script>

<style scoped>
.dashboard {
  padding: 2rem 1rem;
  min-height: calc(100vh - 120px);
  background: linear-gradient(to bottom, #f9fafb 0%, #ffffff 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.welcome-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 3rem;
  padding: 2.5rem;
  background: #93c794;
  border-radius: 1rem;
  color: white;
  animation: slideUp 0.5s ease;
  gap: 2rem;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.15);
}

.welcome-content h1 {
  font-size: 2rem;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 700;
}

.welcome-content p {
  opacity: 0.95;
  margin: 0;
  font-size: 1rem;
}

.welcome-actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 3rem;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 1.75rem;
  transition: all 0.3s ease;
  cursor: default;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.08);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon svg {
  width: 28px;
  height: 28px;
}

.stat-icon.primary { background: var(--primary-light); color: var(--primary); }
.stat-icon.success { background: var(--success-light); color: var(--success); }
.stat-icon.warning { background: var(--warning-light); color: var(--warning); }
.stat-icon.danger { background: var(--danger-light); color: var(--danger); }

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
  line-height: 1;
  margin-bottom: 0.25rem;
}

.stat-label {
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.recent-section h2 {
  font-size: 1.5rem;
  margin-bottom: 1.5rem;
  color: var(--text-primary);
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.activities-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

@media (max-width: 768px) {
  .welcome-section {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .stats-grid {
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  }
  
  .activities-list {
    grid-template-columns: 1fr;
  }
}
</style>
