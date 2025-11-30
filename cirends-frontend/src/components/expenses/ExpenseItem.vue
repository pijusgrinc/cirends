<template>
  <div v-if="expense" class="expense-item" :class="{ 'fully-paid': isFullyPaid }">
    <div class="expense-header">
      <h4 class="expense-title">{{ expense.name }}</h4>
      <span class="expense-amount">{{ formatCurrency(expense.amount) }}</span>
    </div>
    
    <div class="expense-details">
      <div class="expense-detail">
        <span class="label">Sumokėjo:</span>
        <span class="value">{{ expense.paidBy?.name || 'Nežinomas' }}</span>
      </div>
      
      <div class="expense-detail">
        <span class="label">Data:</span>
        <span class="value">{{ formatDateTime(expense.expenseDate || expense.createdAt) }}</span>
      </div>
      
      <div v-if="shares.length > 0" class="expense-shares">
        <span class="label">Paskirstymas:</span>
        <div class="shares-list">
          <div v-for="share in shares" :key="share.id" class="share-item">
            <div class="share-info">
              <span>{{ share.user?.name || `User #${share.userId}` }}: {{ formatCurrency(share.shareAmount) }}</span>
              <span v-if="share.isPaid" class="share-status paid" :title="`Sumokėta: ${formatDateTime(share.paidAt)}`">
                <Icon name="check" :size="14" /> Sumokėta
              </span>
              <span v-else class="share-status unpaid">
                <Icon name="pending" :size="14" /> Nesumokėta
              </span>
            </div>
            <div v-if="canManageAnyAction(share)" class="share-actions">
              <Button 
                v-if="!share.isPaid && canMarkPaid(share)"
                size="sm" 
                variant="ghost" 
                @click.stop="$emit('mark-paid', share.id)"
              >
                Patvirtinti
              </Button>
              <Button 
                v-else-if="share.isPaid && canUnmarkPaid(share)"
                size="sm" 
                variant="ghost" 
                @click.stop="$emit('unmark-paid', share.id)"
              >
                Atšaukti
              </Button>
            </div>
          </div>
        </div>
        <div class="shares-summary">
          <div class="summary-item">
            <span class="summary-label">Viso paskirstyta:</span>
            <span class="summary-value">{{ formatCurrency(totalShares) }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Sumokėta:</span>
            <span class="summary-value success">{{ formatCurrency(paidAmount) }}</span>
          </div>
          <div class="summary-item">
            <span class="summary-label">Liko:</span>
            <span class="summary-value" :class="{ 'warning': remainingAmount > 0 }">{{ formatCurrency(remainingAmount) }}</span>
          </div>
        </div>
      </div>
    </div>
    
    <div v-if="showActions" class="expense-actions">
      <Button size="sm" variant="ghost" @click="$emit('edit', expense)">
        Redaguoti
      </Button>
      <Button size="sm" variant="danger" @click="$emit('delete', expense)">
        Ištrinti
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores'
import { Button } from '@/components/common'
import { Icon } from '@/components/icons'
import { formatCurrency } from '@/utils/currency'
import { formatDateTime } from '@/utils/date'
import type { Expense, ExpenseShare } from '@/types'

interface Props {
  expense: Expense
  showActions?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showActions: false
})

const emit = defineEmits<{
  edit: [expense: Expense]
  delete: [expense: Expense]
  'mark-paid': [shareId: number]
  'unmark-paid': [shareId: number]
}>()

const authStore = useAuthStore()
const currentUserId = computed(() => authStore.user?.id)

// Normalize shares - API returns expenseShares but we use shares internally
const shares = computed(() => props.expense.expenseShares || props.expense.shares || [])

// Check if all shares are paid
const isFullyPaid = computed(() => {
  if (shares.value.length === 0) return false
  return shares.value.every(share => share.isPaid)
})

// Calculate payment summary
const totalShares = computed(() => {
  return shares.value.reduce((sum, share) => sum + share.shareAmount, 0)
})

const paidAmount = computed(() => {
  return shares.value
    .filter(share => share.isPaid)
    .reduce((sum, share) => sum + share.shareAmount, 0)
})

const remainingAmount = computed(() => {
  return totalShares.value - paidAmount.value
})

function canMarkPaid(share: ExpenseShare): boolean {
  // Allow marking as paid by payer or debtor
  return currentUserId.value === props.expense.paidByUserId || currentUserId.value === share.userId
}

function canUnmarkPaid(share: ExpenseShare): boolean {
  return currentUserId.value === props.expense.paidByUserId || currentUserId.value === share.userId
}

function canManageAnyAction(share: ExpenseShare): boolean {
  return canMarkPaid(share) || (share.isPaid && canUnmarkPaid(share))
}
</script>

<style scoped>
.expense-item {
  padding: 1rem;
  background: white;
  border-radius: 0.5rem;
  border: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  transition: opacity 0.2s, background-color 0.2s;
}

.expense-item.fully-paid {
  opacity: 0.6;
  background-color: #f3f4f6;
}

.expense-item.fully-paid .expense-title {
  text-decoration: line-through;
  color: var(--text-secondary);
}

.expense-item.fully-paid .expense-amount {
  color: var(--text-secondary);
}

.expense-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.expense-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.expense-amount {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--success);
}

.expense-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.expense-detail {
  display: flex;
  gap: 0.5rem;
}

.label {
  color: var(--text-secondary);
  font-weight: 500;
}

.value {
  color: var(--text-primary);
}

.expense-shares {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.shares-list {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding-left: 1rem;
}

.share-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem;
  background: #f9fafb;
  border-radius: 0.25rem;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.share-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
  color: var(--text-primary);
}

.share-status {
  padding: 0.125rem 0.5rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  white-space: nowrap;
}

.share-status.paid {
  background-color: var(--success-light);
  color: var(--success-dark);
}

.share-status.unpaid {
  background-color: var(--warning-light);
  color: var(--warning-dark);
}

.share-actions {
  display: flex;
  gap: 0.25rem;
}

.expense-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid #e5e7eb;
}

.shares-summary {
  margin-top: 0.75rem;
  padding: 0.75rem;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.375rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
}

.summary-label {
  color: var(--text-secondary);
  font-weight: 500;
}

.summary-value {
  font-weight: 600;
  color: var(--text-primary);
}

.summary-value.success {
  color: var(--success);
}

.summary-value.warning {
  color: var(--warning);
}
</style>
