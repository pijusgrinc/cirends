<template>
  <div class="my-expenses-page">
    <div class="container">
      <h1>Mano išlaidos</h1>
      <p class="subtitle">Peržiūrėkite savo mokėjimo įsipareigojimus</p>

      <Loading v-if="loading" />

      <div v-else class="expenses-summary">
        <div class="summary-cards">
          <Card class="summary-card unpaid">
            <h3>Nesumokėta</h3>
            <div class="amount">{{ formatCurrency(totalUnpaid) }}</div>
          </Card>
          <Card class="summary-card paid">
            <h3>Sumokėta</h3>
            <div class="amount">{{ formatCurrency(totalPaid) }}</div>
          </Card>
          <Card class="summary-card total">
            <h3>Iš viso</h3>
            <div class="amount">{{ formatCurrency(totalPaid + totalUnpaid) }}</div>
          </Card>
        </div>

        <div v-if="myShares.length > 0" class="shares-list">
          <h2>Išsamios išlaidos</h2>
          <div v-for="group in groupedShares" :key="group.activityId" class="activity-group">
            <h3 class="activity-name">{{ group.activityName }}</h3>
            <div v-for="taskGroup in group.tasks" :key="taskGroup.taskId" class="task-group">
              <h4 class="task-name">{{ taskGroup.taskName }}</h4>
              <div class="expenses-grid">
                <Card v-for="share in taskGroup.shares" :key="share.id" class="expense-card">
                  <div class="expense-header">
                    <h5>{{ share.expenseName }}</h5>
                    <span :class="['expense-status', share.isPaid ? 'paid' : 'unpaid']">
                      <Icon :name="share.isPaid ? 'check' : 'pending'" :size="14" />
                      {{ share.isPaid ? 'Sumokėta' : 'Nesumokėta' }}
                    </span>
                  </div>
                  <div class="expense-details">
                    <div class="detail-row">
                      <span class="label">Jūsų dalis:</span>
                      <span class="value">{{ formatCurrency(share.shareAmount) }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="label">Sumokėjo:</span>
                      <span class="value">{{ share.paidByName }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="label">Išlaidų data:</span>
                      <span class="value">{{ formatDate(share.expenseDate) }}</span>
                    </div>
                  </div>
                </Card>
              </div>
            </div>
          </div>
        </div>

        <EmptyState 
          v-else 
          title="Nėra išlaidų" 
          description="Jūs neturite jokių mokėjimo įsipareigojimų" 
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore, useActivitiesStore } from '@/stores'
import { Loading, EmptyState, Card } from '@/components/common'
import { Icon } from '@/components/icons'
import { formatDate } from '@/utils/date'
import { useToast } from '@/composables'

const authStore = useAuthStore()
const activitiesStore = useActivitiesStore()
const { error } = useToast()

const loading = ref(false)

interface ShareWithContext {
  id: number
  shareAmount: number
  isPaid: boolean
  expenseName: string
  expenseDate: string
  paidByName: string
  activityId: number
  activityName: string
  taskId: number
  taskName: string
}

const myShares = ref<ShareWithContext[]>([])

const totalUnpaid = computed(() => 
  myShares.value.filter(s => !s.isPaid).reduce((sum, s) => sum + s.shareAmount, 0)
)

const totalPaid = computed(() => 
  myShares.value.filter(s => s.isPaid).reduce((sum, s) => sum + s.shareAmount, 0)
)

interface TaskGroup {
  taskId: number
  taskName: string
  shares: ShareWithContext[]
}

interface ActivityGroup {
  activityId: number
  activityName: string
  tasks: TaskGroup[]
}

const groupedShares = computed<ActivityGroup[]>(() => {
  const groups = new Map<number, ActivityGroup>()
  
  for (const share of myShares.value) {
    let activityGroup = groups.get(share.activityId)
    if (!activityGroup) {
      activityGroup = {
        activityId: share.activityId,
        activityName: share.activityName,
        tasks: []
      }
      groups.set(share.activityId, activityGroup)
    }
    
    let taskGroup = activityGroup.tasks.find(t => t.taskId === share.taskId)
    if (!taskGroup) {
      taskGroup = {
        taskId: share.taskId,
        taskName: share.taskName,
        shares: []
      }
      activityGroup.tasks.push(taskGroup)
    }
    
    taskGroup.shares.push(share)
  }
  
  return Array.from(groups.values())
})

function formatCurrency(amount: number): string {
  return `${amount.toFixed(2)} €`
}

async function loadMyExpenses() {
  loading.value = true
  try {
    const currentUserId = authStore.user?.id
    if (!currentUserId) {
      error('Nėra prisijungusio naudotojo')
      return
    }

    // Fetch all activities
    await activitiesStore.fetchActivities()
    const activities = activitiesStore.activities

    const shares: ShareWithContext[] = []

    // For each activity, get tasks and expenses
    for (const activity of activities) {
      // Fetch detailed activity with tasks
      await activitiesStore.fetchActivity(activity.id)
      const detailedActivity = activitiesStore.getActivity(activity.id)
      
      if (!detailedActivity?.tasks) continue

      for (const task of detailedActivity.tasks) {
        if (!task.expenses) continue

        for (const expense of task.expenses) {
          if (!expense.shares) continue

          // Find shares belonging to current user
          const userShares = expense.shares.filter((s: any) => s.userId === currentUserId)
          
          for (const share of userShares) {
            shares.push({
              id: share.id,
              shareAmount: share.shareAmount,
              isPaid: share.isPaid,
              expenseName: expense.name,
              expenseDate: expense.expenseDate,
              paidByName: expense.paidBy?.name || 'Nežinomas',
              activityId: activity.id,
              activityName: activity.name,
              taskId: task.id,
              taskName: task.name
            })
          }
        }
      }
    }

    myShares.value = shares
  } catch (e) {
    error('Nepavyko gauti išlaidų')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadMyExpenses()
})
</script>

<style scoped>
.my-expenses-page {
  padding: 2rem 0;
  min-height: calc(100vh - 120px);
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

h1 {
  font-size: 2rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: var(--text-secondary);
  margin-bottom: 2rem;
}

.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.summary-card {
  padding: 1.5rem;
  text-align: center;
}

.summary-card h3 {
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.5rem;
  color: var(--text-secondary);
}

.summary-card .amount {
  font-size: 2rem;
  font-weight: 700;
}

.summary-card.unpaid .amount {
  color: var(--danger);
}

.summary-card.paid .amount {
  color: var(--success);
}

.summary-card.total .amount {
  color: var(--primary);
}

.shares-list h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin-bottom: 1.5rem;
}

.activity-group {
  margin-bottom: 2rem;
}

.activity-name {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 1rem;
  color: var(--primary);
}

.task-group {
  margin-bottom: 1.5rem;
  margin-left: 1rem;
}

.task-name {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 1rem;
  color: var(--text-secondary);
}

.expenses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1rem;
  margin-left: 1rem;
}

.expense-card {
  padding: 1rem;
}

.expense-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  gap: 0.5rem;
}

.expense-header h5 {
  font-size: 1rem;
  font-weight: 600;
  margin: 0;
  flex: 1;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 600;
  white-space: nowrap;
}

.status-badge.paid {
  background: var(--success-light);
  color: var(--success);
}

.status-badge.unpaid {
  background: var(--warning-light);
  color: var(--warning);
}

.expense-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

.detail-row .label {
  color: var(--text-secondary);
}

.detail-row .value {
  font-weight: 600;
}
</style>
