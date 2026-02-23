<template>
  <form class="form" @submit.prevent="onSubmit">
    <div class="form-row">
      <label for="name">Pavadinimas</label>
      <input id="name" v-model.trim="form.name" type="text" required placeholder="Išlaidos pavadinimas" />
    </div>

    <div class="form-row">
      <label for="amount">Suma (€)</label>
      <input id="amount" v-model.number="form.amount" type="number" step="0.01" min="0.01" required />
    </div>

    <div class="form-row">
      <label for="expenseDate">Data</label>
      <input id="expenseDate" v-model="form.expenseDate" type="datetime-local" required />
    </div>

    <div class="form-row">
      <label for="paidBy">Kas sumokėjo</label>
      <select id="paidBy" v-model.number="form.paidByUserId" required>
        <option value="0" disabled>Pasirinkite...</option>
        <option v-for="p in participants" :key="p.id" :value="p.id">{{ p.name }}</option>
      </select>
    </div>

    <div class="form-row">
      <label>Paskirstymas</label>
      <div v-if="form.shares.length > 0 && form.amount > 0" class="distribution-summary">
        <span>Paskirstyta: {{ totalAssigned.toFixed(2) }} €</span>
        <span :class="['remaining', { 'fully-distributed': isFullyDistributed, 'has-remaining': !isFullyDistributed }]">
          Likutis: {{ remainingAmount.toFixed(2) }} €
        </span>
      </div>
      <div class="shares">
        <div class="shares-actions">
          <Button size="sm" variant="ghost" @click.prevent="addShare" :disabled="availableParticipants.length === 0">Pridėti dalį</Button>
          <Button size="sm" variant="ghost" @click.prevent="splitEqually" :disabled="form.shares.length === 0">Padalinti po lygiai</Button>
        </div>
        <div v-for="(share, i) in form.shares" :key="i" class="share-row">
          <select v-model.number="share.userId" required @change="onShareUserChange(i)">
            <option v-for="p in getAvailableParticipantsForShare(i)" :key="p.id" :value="p.id">{{ p.name }}</option>
          </select>
          <input 
            v-model.number="share.shareAmount" 
            type="number" 
            step="0.01" 
            min="0" 
            placeholder="Suma" 
            required 
            @input="onShareAmountChange(i)"
          />
          <Button size="sm" variant="danger" @click.prevent="removeShare(i)">Šalinti</Button>
        </div>
      </div>
    </div>

    <div class="actions">
      <Button type="submit" variant="primary" :disabled="loading">{{ submitLabel }}</Button>
      <Button type="button" variant="ghost" @click="$emit('cancel')">Atšaukti</Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, computed, watch } from 'vue'
import { useExpensesStore } from '@/stores'
import { Button } from '@/components/common'
import { toDateTimeLocal } from '@/utils/date'
import type { CreateExpenseRequest, UpdateExpenseRequest, User, Expense } from '@/types'

interface Props {
  participants: User[]
  expense?: Expense | null
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  expense: null,
  loading: false
})

const emit = defineEmits<{
  submit: [payload: CreateExpenseRequest | UpdateExpenseRequest]
  cancel: []
}>()

const expensesStore = useExpensesStore()

// Helper to get shares from expense (handles both shares and expenseShares)
function getExpenseShares(expense: Expense | null | undefined): { userId: number; shareAmount: number }[] {
  if (!expense) return []
  const shares = (expense as any).expenseShares || expense.shares || []
  return shares.map((s: any) => ({ 
    userId: s.userId, 
    shareAmount: s.shareAmount 
  }))
}

interface ShareForm {
  userId: number
  shareAmount: number
}

const form = reactive<{
  name: string
  amount: number
  currency: string
  expenseDate: string
  paidByUserId: number
  shares: ShareForm[]
}>({
  name: props.expense?.name || '',
  amount: props.expense?.amount ?? 0,
  currency: props.expense?.currency ?? 'EUR',
  expenseDate: toDateTimeLocal(props.expense?.expenseDate) || '',
  paidByUserId: props.expense?.paidByUserId ?? expensesStore.lastPaidByUserId ?? 0,
  shares: getExpenseShares(props.expense)
})

// Watch expense changes for edit mode
watch(() => props.expense, (exp) => {
  if (exp) {
    form.name = exp.name
    form.amount = exp.amount
    form.currency = exp.currency ?? 'EUR'
    form.expenseDate = toDateTimeLocal(exp.expenseDate) || ''
    form.paidByUserId = exp.paidByUserId
    form.shares = getExpenseShares(exp)
  }
}, { immediate: true })

const submitLabel = computed(() => (props.expense ? 'Išsaugoti' : 'Sukurti'))

// Calculate remaining amount to distribute
const totalAssigned = computed(() => 
  form.shares.reduce((sum, s) => sum + (s.shareAmount || 0), 0)
)

const remainingAmount = computed(() => 
  Number((form.amount - totalAssigned.value).toFixed(2))
)

const isFullyDistributed = computed(() => 
  Math.abs(remainingAmount.value) < 0.01
)

// Get participants that haven't been assigned yet (excluding current share's selection)
const availableParticipants = computed(() => {
  const assignedIds = form.shares.map(s => s.userId)
  return props.participants.filter(p => !assignedIds.includes(p.id))
})

// Get available participants for a specific share (includes already selected user for that share)
function getAvailableParticipantsForShare(shareIndex: number): User[] {
  const currentUserId = form.shares[shareIndex]?.userId
  const otherAssignedIds = form.shares
    .filter((_, i) => i !== shareIndex)
    .map(s => s.userId)
  
  return props.participants.filter(p => 
    p.id === currentUserId || !otherAssignedIds.includes(p.id)
  )
}

function addShare() {
  const available = availableParticipants.value
  if (available.length === 0) return
  
  // Calculate remaining amount
  const assigned = form.shares.reduce((sum, s) => sum + (s.shareAmount || 0), 0)
  const remaining = Math.max(0, form.amount - assigned)
  
  form.shares.push({ userId: available[0]!.id, shareAmount: Number(remaining.toFixed(2)) })
}

function onShareUserChange(shareIndex: number) {
  // No additional logic needed, just reactive update
}

function onShareAmountChange(changedIndex: number) {
  if (form.shares.length <= 1) return
  
  // Calculate total assigned to shares
  const assigned = form.shares.reduce((sum, s) => sum + (s.shareAmount || 0), 0)
  const remaining = form.amount - assigned
  
  // If there's remaining amount, distribute it to other shares
  if (Math.abs(remaining) > 0.01) {
    const otherShares = form.shares.filter((_, i) => i !== changedIndex)
    if (otherShares.length > 0) {
      const perShare = remaining / otherShares.length
      form.shares.forEach((share, i) => {
        if (i !== changedIndex) {
          share.shareAmount = Math.max(0, Number(((share.shareAmount || 0) + perShare).toFixed(2)))
        }
      })
    }
  }
}

function removeShare(index: number) {
  form.shares.splice(index, 1)
}

function splitEqually() {
  if (form.shares.length === 0 || form.amount <= 0) return
  
  const baseAmount = Math.floor((form.amount / form.shares.length) * 100) / 100
  const totalBase = baseAmount * form.shares.length
  const remainder = Number((form.amount - totalBase).toFixed(2))
  
  form.shares.forEach((share, i) => {
    // Add the remainder to the first share to account for rounding
    share.shareAmount = i === 0 ? Number((baseAmount + remainder).toFixed(2)) : baseAmount
  })
}

function onSubmit() {
  const payload = {
    name: form.name,
    amount: form.amount,
    currency: form.currency,
    expenseDate: form.expenseDate,
    paidByUserId: form.paidByUserId,
    shares: form.shares
  }
  emit('submit', payload)
}
</script>

<style scoped>
.shares {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.shares-actions {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.share-row {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.distribution-summary {
  display: flex;
  gap: 1rem;
  padding: 0.75rem;
  background: var(--gray-50);
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text-secondary);
}

.distribution-summary .remaining {
  margin-left: auto;
}

.distribution-summary .fully-distributed {
  color: var(--success);
}

.distribution-summary .has-remaining {
  color: var(--warning);
}
</style>