<template>
  <div class="activity-detail-page">
    <div class="container">
      <Button variant="ghost" size="sm" @click="$router.push('/activities')" class="back-button">
        ← Grįžti į veiklas
      </Button>
      
      <Loading v-if="activitiesStore.loading" />

      <div v-else-if="activity" class="activity-content">
        <div class="activity-header">
          <div class="activity-title-section">
            <h1>{{ activity.name }}</h1>
            <div class="activity-badges">
              <span class="badge">
                <Icon name="location" :size="16" />
                {{ activity.location || 'Nenurodyta' }}
              </span>
              <span class="badge">
                <Icon name="calendar" :size="16" />
                {{ formatDate(activity.startDate) }}
              </span>
            </div>
          </div>
          <div class="activity-header-actions">
            <Button size="sm" variant="ghost" @click="editActivity">
              Redaguoti
            </Button>
            <Button size="sm" variant="danger" @click="deleteActivity">
              Ištrinti
            </Button>
          </div>
        </div>

        <p class="activity-description">{{ activity.description }}</p>

        <div class="tabs-section">
          <div class="tabs">
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

          <!-- Tasks Tab (nested expenses) -->
          <div v-if="activeTab === 'tasks'" class="tab-content">
            <div class="tab-header">
              <h2>Užduotys</h2>
              <Button variant="primary" @click="showTaskModal = true">
                <template #icon>
                  <Icon name="add" />
                </template>
                Nauja užduotis
              </Button>
            </div>

            <div v-if="tasks.length > 0" class="tasks-list">
              <div v-for="task in tasks" :key="task.id" class="task-with-expenses">
                <TaskItem
                  :task="task"
                  @edit="editTask(task)"
                  @delete="deleteTask(task.id)"
                  @update-status="(taskObj, status) => updateTaskStatus(task.id, status)"
                  @mark-expenses-paid="handleMarkExpensesPaid"
                />
                <div class="task-expenses">
                  <div class="task-expenses-header">
                    <h5>Išlaidos</h5>
                    <Button size="sm" variant="ghost" @click="openExpenseForTask(task.id)">+ Pridėti išlaidas</Button>
                  </div>
                  <div v-if="expensesStore.getExpenses(activityId, task.id).length > 0" class="expenses-list">
                    <ExpenseItem
                      v-for="expense in expensesStore.getExpenses(activityId, task.id)"
                      :key="expense.id"
                      :expense="expense"
                      :showActions="true"
                      @edit="editExpense(expense)"
                      @delete="deleteExpense(expense.id)"
                      @mark-paid="(shareId) => handleMarkSharePaid(expense, shareId)"
                      @unmark-paid="(shareId) => handleUnmarkSharePaid(expense, shareId)"
                    />
                  </div>
                  <EmptyState v-else title="Nėra išlaidų" description="Pridėkite išlaidas šiai užduočiai" />
                </div>
              </div>
            </div>
            <EmptyState v-else title="Nėra užduočių" description="Sukurkite pirmąją užduotį" />
          </div>

          <!-- Participants Tab -->
          <div v-if="activeTab === 'participants'" class="tab-content">
            <div class="tab-header">
              <h2>Dalyviai</h2>
              <Button variant="primary" @click="showInviteModal = true">
                <template #icon>
                  <Icon name="add" />
                </template>
                Pakviesti
              </Button>
            </div>

            <div class="participants-table-wrapper">
              <table class="participants-table">
                <thead>
                  <tr>
                    <th>Vardas</th>
                    <th>El. paštas</th>
                    <th>Rolė</th>
                    <th>Prisijungė</th>
                    <th>Veiksmai</th>
                  </tr>
                </thead>
                <tbody>
                  <!-- Creator Row -->
                  <tr v-if="activity?.createdBy" class="creator-row">
                    <td>
                      <div class="participant-name">
                        <div class="participant-avatar">{{ getInitials(activity.createdBy.name) }}</div>
                        <strong>{{ activity.createdBy.name }}</strong>
                      </div>
                    </td>
                    <td>{{ activity.createdBy.email }}</td>
                    <td>
                      <span class="role-badge role-creator">Kūrėjas</span>
                    </td>
                    <td>{{ formatDate(activity.createdAt) }}</td>
                    <td>
                      <span class="muted-text">Negalima pašalinti</span>
                    </td>
                  </tr>

                  <!-- Active Participants from ActivityUsers -->
                  <tr 
                    v-for="participant in activeParticipants" 
                    :key="'participant-' + participant.userId"
                  >
                    <td>
                      <div class="participant-name">
                        <div class="participant-avatar">{{ getInitials(participant.user?.name) }}</div>
                        <strong>{{ participant.user?.name }}</strong>
                      </div>
                    </td>
                    <td>{{ participant.user?.email }}</td>
                    <td>
                      <span class="role-badge role-member">Narys</span>
                    </td>
                    <td>
                      <span class="invited-by">{{ formatDate(participant.joinedAt) }}</span>
                    </td>
                    <td>
                      <Button 
                        v-if="participant.userId && canRemoveParticipant(participant.userId)"
                        size="sm" 
                        :variant="participant.userId === currentUserId ? 'ghost' : 'danger'"
                        @click="removeParticipant(participant.userId)"
                        :disabled="removingParticipant"
                      >
                        {{ getRemoveButtonText(participant.userId) }}
                      </Button>
                      <span v-else class="muted-text">—</span>
                    </td>
                  </tr>

                  <!-- Pending Invitations -->
                  <tr 
                    v-for="invitation in pendingInvitations" 
                    :key="'pending-' + invitation.id"
                    class="pending-row"
                  >
                    <td>
                      <div class="participant-name">
                        <div class="participant-avatar pending">?</div>
                        <span>{{ invitation.invitedUser?.name || 'Laukiama' }}</span>
                      </div>
                    </td>
                    <td>{{ invitation.invitedUser?.email }}</td>
                    <td>
                      <span class="role-badge role-pending">Laukiama</span>
                    </td>
                    <td>{{ formatDate(invitation.createdAt) }}</td>
                    <td>
                      <span class="muted-text">Pakvietimas nepatvirtintas</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            
            <EmptyState 
              v-if="!activity?.createdBy && activityInvitations.length === 0"
              title="Nėra dalyvių" 
              description="Pakvieskite narius prisijungti prie šios veiklos" 
            />
          </div>
        </div>
      </div>

      <EmptyState
        v-else
        title="Veikla nerasta"
        description="Ši veikla neegzistuoja arba buvo ištrinta"
      >
        <Button variant="primary" @click="$router.push('/activities')">
          Grįžti į veiklas
        </Button>
      </EmptyState>
    </div>

    <!-- Task Modal -->
    <Modal v-if="showTaskModal" @close="closeTaskModal">
      <template #header>{{ editingTask ? 'Redaguoti užduotį' : 'Nauja užduotis' }}</template>
      <template #body>
        <TaskForm
          :task="editingTask"
          :loading="tasksStore.loading"
          @submit="handleSubmitTask"
          @cancel="closeTaskModal"
        />
      </template>
    </Modal>

    <!-- Expense Modal -->
    <Modal v-if="showExpenseModal" @close="closeExpenseModal">
      <template #header>{{ editingExpense ? 'Redaguoti išlaidas' : 'Nauja išlaida' }}</template>
      <template #body>
        <ExpenseForm
          :tasks="tasks"
          :participants="participants"
          :expense="editingExpense"
          :initialTaskId="selectedTaskForExpense"
          :loading="expensesStore.loading"
          @submit="handleSubmitExpense"
          @cancel="closeExpenseModal"
        />
      </template>
    </Modal>

    <!-- Activity Edit Modal -->
    <Modal v-if="showActivityModal" @close="showActivityModal = false">
      <template #header>Redaguoti veiklą</template>
      <template #body>
        <ActivityForm
          :activity="activity"
          :loading="activitiesStore.loading"
          @submit="handleActivitySubmit"
          @cancel="showActivityModal = false"
        />
      </template>
    </Modal>

    <!-- Invite Modal -->
    <Modal v-if="showInviteModal" @close="closeInviteModal">
      <template #header>Pakviesti dalyvį</template>
      <template #body>
        <form class="modal-form" @submit.prevent="handleInviteSubmit">
          <div class="form-row">
            <label for="invite-email">El. paštas</label>
            <input
              id="invite-email"
              v-model.trim="inviteEmail"
              type="email"
              required
              placeholder="vardas@example.com"
              :disabled="inviteLoading"
            />
          </div>
          <div class="form-actions">
            <Button type="submit" variant="primary" :disabled="inviteLoading || !inviteEmail">
              {{ inviteLoading ? 'Siunčiama...' : 'Siųsti pakvietimą' }}
            </Button>
            <Button type="button" variant="ghost" @click="closeInviteModal">Atšaukti</Button>
          </div>
        </form>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useActivitiesStore, useTasksStore, useExpensesStore } from '@/stores'
import { useToast } from '@/composables'
import { formatDate } from '@/utils/date'
import { Button, Loading, EmptyState, Card } from '@/components/common'
import { TaskItem, TaskForm } from '@/components/tasks'
import { ExpenseItem, ExpenseForm } from '@/components/expenses'
import { ActivityForm } from '@/components/activities'
import { Icon } from '@/components/icons'
import Modal from '@/components/Modal.vue'
import type { Task, Expense, TaskStatus } from '@/types'
import { getRoleLabel } from '@/types/enums'
import { invitationsAPI, expensesAPI, activitiesAPI } from '@/api'

const route = useRoute()
const $router = useRouter()
const activitiesStore = useActivitiesStore()
const tasksStore = useTasksStore()
const expensesStore = useExpensesStore()
const { success, error } = useToast()

// Import auth store to get current user
import { useAuthStore } from '@/stores'
const authStore = useAuthStore()
const currentUserId = computed(() => authStore.user?.id)

function getInvitationStatusLabel(status: number): string {
  const labels: Record<number, string> = {
    0: 'Laukiama',
    1: 'Priimta',
    2: 'Atmesta'
  }
  return labels[status] || 'Nežinoma'
}

const activityId = computed(() => Number(route.params.id))
const activity = computed(() => activitiesStore.getActivity(activityId.value))
const tasks = computed(() => tasksStore.getTasks(activityId.value))
// Aggregated expenses no longer needed for tab view; kept for potential summary use
const expenses = computed(() => tasks.value.flatMap(task => expensesStore.getExpenses(activityId.value, task.id)))
// Only invited participants should be available for expense shares
// Extract User objects from ActivityUser array and add creator
const participants = computed(() => {
  if (!activity.value) return []
  const users: any[] = []
  
  // Add creator first
  if (activity.value.createdBy) {
    users.push(activity.value.createdBy)
  }
  
  // Add participants
  if (activity.value.participants) {
    activity.value.participants
      .map((p: any) => p.user)
      .filter((u: any): u is NonNullable<typeof u> => u != null)
      .forEach((u: any) => {
        // Avoid duplicates
        if (!users.some(existing => existing.id === u.id)) {
          users.push(u)
        }
      })
  }
  
  return users
})

const activeTab = ref<'tasks' | 'participants'>('tasks')
const showTaskModal = ref(false)
const showExpenseModal = ref(false)
const showInviteModal = ref(false)
const editingTask = ref<Task | null>(null)
const editingExpense = ref<Expense | null>(null)
const selectedTaskForExpense = ref<number>(0)
const showActivityModal = ref(false)
const inviteEmail = ref('')
const inviteLoading = ref(false)
const activityInvitations = ref<any[]>([])
const invitationsLoading = ref(false)
const removingParticipant = ref(false)

const tabs = [
  { key: 'tasks' as const, label: 'Užduotys' },
  { key: 'participants' as const, label: 'Dalyviai' }
]

// Active participants from ActivityUsers (not invitations)
const activeParticipants = computed(() => {
  return activity.value?.participants || []
})

const pendingInvitations = computed(() => {
  return activityInvitations.value.filter(inv => inv.status === 0) // Pending
})

// Check if current user is the activity creator
const isActivityCreator = computed(() => {
  return activity.value?.createdBy?.id === currentUserId.value
})

// Check if user can remove a participant
function canRemoveParticipant(userId: number): boolean {
  // Cannot remove the creator
  if (activity.value?.createdBy?.id === userId) return false
  
  // Can remove yourself
  if (userId === currentUserId.value) return true
  
  // Only creator can remove others
  return isActivityCreator.value
}

// Get button text based on whether removing self or others
function getRemoveButtonText(userId: number): string {
  return userId === currentUserId.value ? 'Išeiti' : 'Pašalinti'
}

// Get confirmation message based on whether removing self or others
function getRemoveConfirmation(userId: number): string {
  return userId === currentUserId.value 
    ? 'Ar tikrai norite išeiti iš veiklos?' 
    : 'Ar tikrai norite pašalinti šį dalyvį iš veiklos?'
}

async function loadActivityData(forceRefresh = false) {
  try {
    await activitiesStore.fetchActivity(activityId.value, forceRefresh)
    await tasksStore.fetchTasks(activityId.value, forceRefresh)
    
    // Fetch expenses for all tasks
    for (const task of tasks.value) {
      await expensesStore.fetchExpenses(activityId.value, task.id, forceRefresh)
    }
    
    // Fetch invitations for this activity
    await loadActivityInvitations()
  } catch (e) {
    error('Nepavyko gauti veiklos duomenų')
  }
}

async function loadActivityInvitations() {
  invitationsLoading.value = true
  try {
    const response = await invitationsAPI.getActivityInvitations(activityId.value)
    if (response.ok && response.data) {
      activityInvitations.value = response.data
    }
  } catch (e) {
    console.error('Failed to load invitations:', e)
  } finally {
    invitationsLoading.value = false
  }
}

onMounted(() => {
  loadActivityData()
})

// Watch for route changes to reload data
watch(() => route.params.id, (newId, oldId) => {
  if (newId && newId !== oldId) {
    loadActivityData(true)
  }
})

function getInitials(name: string | undefined): string {
  if (!name) return '?'
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .substring(0, 2)
}

function editTask(task: Task) {
  editingTask.value = task
  showTaskModal.value = true
}

function closeTaskModal() {
  showTaskModal.value = false
  editingTask.value = null
}

async function deleteTask(taskId: number) {
  if (!confirm('Ar tikrai norite ištrinti šią užduotį?')) return
  
  try {
    await tasksStore.deleteTask(activityId.value, taskId)
    success('Užduotis ištrinta')
    await loadActivityData(true) // Refresh data
  } catch (e) {
    error('Nepavyko ištrinti užduoties')
  }
}

async function updateTaskStatus(taskId: number, newStatus: TaskStatus) {
  try {
    await tasksStore.updateTaskStatus(activityId.value, taskId, newStatus)
    success('Užduoties būsena atnaujinta')
  } catch (e) {
    error('Nepavyko atnaujinti užduoties')
  }
}

function editExpense(expense: Expense) {
  editingExpense.value = expense
  selectedTaskForExpense.value = expense.taskId
  showExpenseModal.value = true
}

function closeExpenseModal() {
  showExpenseModal.value = false
  editingExpense.value = null
  selectedTaskForExpense.value = 0
}

async function deleteExpense(expenseId: number) {
  if (!confirm('Ar tikrai norite ištrinti šią išlaidą?')) return
  
  try {
    // Find which task this expense belongs to
    const task = tasks.value.find(t => 
      expensesStore.getExpenses(activityId.value, t.id).some(e => e.id === expenseId)
    )
    
    if (task) {
      await expensesStore.deleteExpense(activityId.value, task.id, expenseId)
      success('Išlaida ištrinta')
      await loadActivityData(true) // Refresh data
    }
  } catch (e) {
    error('Nepavyko ištrinti išlaidos')
  }
}

async function handleSubmitTask(payload: any) {
  try {
    if (editingTask.value) {
      const updated = await tasksStore.updateTask(activityId.value, editingTask.value.id, payload)
      if (updated) {
        success('Užduotis atnaujinta!')
        closeTaskModal()
        await loadActivityData(true) // Refresh data
      } else {
        error(tasksStore.error || 'Nepavyko atnaujinti užduoties')
      }
    } else {
      const created = await tasksStore.createTask(activityId.value, payload)
      if (created) {
        success('Užduotis sukurta!')
        closeTaskModal()
        await loadActivityData(true) // Refresh data
      } else {
        error(tasksStore.error || 'Nepavyko sukurti užduoties')
      }
    }
  } catch (e) {
    error('Nepavyko išsaugoti užduoties')
  }
}

function openExpenseForTask(taskId: number) {
  selectedTaskForExpense.value = taskId
  editingExpense.value = null
  showExpenseModal.value = true
}

async function handleSubmitExpense(payload: any) {
  try {
    const { taskId, ...data } = payload
    const targetTaskId = taskId || selectedTaskForExpense.value
    if (!targetTaskId) {
      error('Pasirinkite užduotį')
      return
    }
    if (editingExpense.value) {
      const updated = await expensesStore.updateExpense(activityId.value, targetTaskId, editingExpense.value.id, data)
      if (updated) {
        success('Išlaida atnaujinta!')
        closeExpenseModal()
        await loadActivityData(true) // Refresh data
      } else {
        error(expensesStore.error || 'Nepavyko atnaujinti išlaidos')
      }
    } else {
      const created = await expensesStore.createExpense(activityId.value, targetTaskId, data)
      if (created) {
        success('Išlaida sukurta!')
        closeExpenseModal()
        await loadActivityData(true) // Refresh data
      } else {
        error(expensesStore.error || 'Nepavyko sukurti išlaidos')
      }
    }
  } catch (e) {
    error('Nepavyko išsaugoti išlaidos')
  }
}

function editActivity() {
  showActivityModal.value = true
}

async function deleteActivity() {
  if (!confirm('Ar tikrai norite ištrinti šią veiklą?')) return
  
  try {
    const deleted = await activitiesStore.deleteActivity(activityId.value)
    if (deleted) {
      success('Veikla ištrinta')
      $router.push('/activities')
    } else {
      error(activitiesStore.error || 'Nepavyko ištrinti veiklos')
    }
  } catch (e) {
    error('Nepavyko ištrinti veiklos')
  }
}

async function handleActivitySubmit(data: any) {
  try {
    const updated = await activitiesStore.updateActivity(activityId.value, data)
    if (updated) {
      success('Veikla atnaujinta!')
      showActivityModal.value = false
    } else {
      error(activitiesStore.error || 'Nepavyko atnaujinti veiklos')
    }
  } catch (e) {
    error('Nepavyko atnaujinti veiklos')
  }
}

async function handleMarkSharePaid(expense: Expense, shareId: number) {
  try {
    const result = await expensesStore.markShareAsPaid(activityId.value, expense.taskId, expense.id, shareId)
    if (result) {
      success('Mokėjimas patvirtintas!')
      await loadActivityData(true)
    } else {
      error(expensesStore.error || 'Nepavyko patvirtinti mokėjimo')
    }
  } catch (e) {
    error('Nepavyko patvirtinti mokėjimo')
  }
}

async function handleUnmarkSharePaid(expense: Expense, shareId: number) {
  try {
    const result = await expensesStore.unmarkShareAsPaid(activityId.value, expense.taskId, expense.id, shareId)
    if (result) {
      success('Mokėjimo patvirtinimas atšauktas')
      await loadActivityData(true)
    } else {
      error(expensesStore.error || 'Nepavyko atšaukti patvirtinimo')
    }
  } catch (e) {
    error('Nepavyko atšaukti patvirtinimo')
  }
}

async function handleMarkExpensesPaid(task: Task) {
  if (!task.expenses || task.expenses.length === 0) return
  
  // Count unpaid shares
  const unpaidCount = task.expenses.reduce((count, expense) => {
    return count + (expense.shares?.filter(s => !s.isPaid).length || 0)
  }, 0)
  
  if (unpaidCount === 0) return // All already paid
  
  const confirmed = confirm(
    `Užduotis „${task.name}" pažymėta kaip atlikta.\n\n` +
    `Ar norite pažymėti visas susijusias išlaidas (${unpaidCount} mokėjimų) kaip sumokėtas?`
  )
  
  if (!confirmed) return
  
  try {
    const response = await expensesAPI.markAllExpensesAsPaidForTask(activityId.value, task.id)
    if (response.ok) {
      success('Visos išlaidos pažymėtos kaip sumokėtos!')
      await loadActivityData(true)
    } else {
      error('Nepavyko pažymėti išlaidų kaip sumokėtų')
    }
  } catch (e) {
    error('Netikėta klaida')
  }
}

async function handleInviteSubmit() {
  if (!inviteEmail.value) return
  
  inviteLoading.value = true
  
  try {
    const response = await invitationsAPI.send(inviteEmail.value.toLowerCase(), activityId.value)
    
    if (response.ok) {
      success(`Pakvietimas išsiųstas į ${inviteEmail.value}`)
      closeInviteModal()
      // Refresh activity to get updated participants
      await loadActivityData(true)
    } else {
      error(response.error?.message || 'Nepavyko išsiųsti pakvietimo')
    }
  } catch (e) {
    error('Netikėta klaida siunčiant pakvietimą')
    console.error('Invite error:', e)
  } finally {
    inviteLoading.value = false
  }
}

function closeInviteModal() {
  showInviteModal.value = false
  inviteEmail.value = ''
  inviteLoading.value = false
}

async function removeParticipant(userId: number) {
  if (!confirm(getRemoveConfirmation(userId))) return
  
  removingParticipant.value = true
  
  try {
    // Use store action so caches stay consistent
    const ok = await activitiesStore.removeParticipant(activityId.value, userId)
    
    if (ok) {
      const isLeavingSelf = userId === currentUserId.value
      success(isLeavingSelf ? 'Sėkmingai išėjote iš veiklos' : 'Dalyvis pašalintas iš veiklos')
      
      // If user left themselves, redirect to activities list
      if (isLeavingSelf) {
        // Clear current activity and refresh list to avoid stale view
        activitiesStore.clearCurrentActivity()
        await activitiesStore.fetchActivities(true)
        $router.push('/activities')
      } else {
        await loadActivityData(true)
      }
    } else {
      error('Nepavyko pašalinti dalyvio')
    }
  } catch (e) {
    error('Netikėta klaida šalinant dalyvį')
    console.error('Remove participant error:', e)
  } finally {
    removingParticipant.value = false
  }
}
</script>

<style scoped>
.activity-detail-page {
  padding: 2rem 1rem;
  min-height: calc(100vh - 120px);
  background: linear-gradient(to bottom, #fafbfc 0%, #ffffff 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.back-button {
  margin-bottom: 1.5rem;
}

.activity-header {
  margin-bottom: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.activity-title-section {
  flex: 1;
}

.activity-header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 0.75rem;
}

.activity-header-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
}

.activity-badges {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.75rem;
  background: var(--gray-100);
  border-radius: var(--border-radius-sm);
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.activity-description {
  color: var(--text-secondary);
  margin-bottom: 2rem;
  line-height: 1.6;
}

.tabs-section {
  margin-top: 2rem;
}

.tabs {
  display: flex;
  gap: 0.5rem;
  border-bottom: 2px solid var(--border);
  margin-bottom: 2rem;
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

.tab-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.tab-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-primary);
}

.tasks-list, .expenses-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.task-with-expenses {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border);
}

.task-expenses {
  margin-left: 2.25rem; /* indent under checkbox area of TaskItem */
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.task-expenses-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.task-expenses-header h5 {
  margin: 0;
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.participants-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1rem;
}

.participant-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
}

.participant-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--primary-gradient);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 1.125rem;
}

.participant-info {
  flex: 1;
}

.participant-info h4 {
  font-size: 1rem;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 0.25rem;
}

.participant-info p {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0;
}

.role-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 0.375rem;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.role-badge.role-0 { background: var(--gray-100); color: var(--text-secondary); }
.role-badge.role-1 { background: var(--primary-light); color: var(--primary); }
.role-badge.role-2 { background: var(--danger-light); color: var(--danger); }

/* Participants Table */
.participants-table-wrapper {
  overflow-x: auto;
  background: white;
  border: 1px solid var(--border);
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.participants-table {
  width: 100%;
  border-collapse: collapse;
}

.participants-table thead {
  background: linear-gradient(to bottom, var(--gray-50), var(--gray-100));
  border-bottom: 2px solid var(--border);
}

.participants-table th {
  padding: 1rem 1.25rem;
  text-align: left;
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.participants-table th:first-child {
  border-top-left-radius: 12px;
}

.participants-table th:last-child {
  border-top-right-radius: 12px;
}

.participants-table td {
  padding: 1.25rem 1.25rem;
  border-bottom: 1px solid var(--border);
  font-size: 0.9375rem;
  color: var(--text-primary);
  vertical-align: middle;
}

.participants-table tbody tr:last-child td {
  border-bottom: none;
}

.participants-table tbody tr:hover {
  background: var(--gray-50);
  transition: background 0.2s ease;
}

.participants-table tbody tr.creator-row {
  background: linear-gradient(to right, rgba(59, 130, 246, 0.08), rgba(59, 130, 246, 0.03));
}

.participants-table tbody tr.creator-row:hover {
  background: linear-gradient(to right, rgba(59, 130, 246, 0.12), rgba(59, 130, 246, 0.06));
}

.participants-table tbody tr.pending-row {
  opacity: 0.65;
  background: var(--gray-50);
}

.participant-name {
  display: flex;
  align-items: center;
  gap: 0.875rem;
}

.participant-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--primary-gradient);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.9375rem;
  flex-shrink: 0;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.participant-avatar.pending {
  background: var(--gray-300);
  color: var(--text-secondary);
}

.role-badge {
  padding: 0.375rem 0.875rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  white-space: nowrap;
  letter-spacing: 0.025em;
}

.role-badge.role-creator {
  background: #2f7939d3;
  color: white;
  box-shadow: 0 2px 4px rgba(59, 130, 246, 0.2);
}

.role-badge.role-member {
  background: var(--success-light);
  color: var(--success);
  border: 1px solid var(--success);
}

.role-badge.role-pending {
  background: var(--warning-light);
  color: var(--warning);
  border: 1px solid var(--warning);
}

.invited-by {
  color: var(--text-secondary);
  font-size: 0.875rem;
}

.muted-text {
  color: var(--text-secondary);
  font-size: 0.875rem;
  font-style: italic;
}

@media (max-width: 768px) {
  .tab-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
  
  .participants-list {
    grid-template-columns: 1fr;
  }

  .participants-table-wrapper {
    overflow-x: scroll;
  }

  .participants-table {
    min-width: 600px;
  }
}
</style>
