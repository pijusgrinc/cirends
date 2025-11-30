import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { expensesAPI } from '@/api'
import type { Expense, CreateExpenseRequest, UpdateExpenseRequest, ExpenseSummary } from '@/types'
import { toUtcIso } from '@/utils/date'

/**
 * Išlaidų store
 * Išlaidos yra žemiausias hierarchijos lygis: Veikla → Užduotys → Išlaidos
 * Kiekviena išlaida priklauso užduočiai ir gali būti paskirstyta tarp dalyvių
 */
export const useExpensesStore = defineStore('expenses', () => {
  // State - expenses pagal activityId ir taskId
  const expensesByTask = ref<Record<string, Expense[]>>({})
  const currentExpense = ref<Expense | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const lastPaidByUserId = ref<number | null>(null)

  // Helper funkcija raktui generuoti
  const getKey = (activityId: number, taskId: number) => `${activityId}-${taskId}`

  // Getters
  const getExpenses = computed(() => (activityId: number, taskId: number) => {
    const key = getKey(activityId, taskId)
    return expensesByTask.value[key] || []
  })

  const getTotalExpenses = computed(() => (activityId: number, taskId: number) => {
    const expenses = getExpenses.value(activityId, taskId)
    return expenses.reduce((sum, exp) => sum + exp.amount, 0)
  })

  const getExpenseSummary = computed(() => (activityId: number, taskId: number, userId: number): ExpenseSummary => {
    const expenses = getExpenses.value(activityId, taskId)
    
    let myShare = 0
    let myPaid = 0
    
    expenses.forEach(expense => {
      // Kiek aš sumokėjau
      if (expense.paidByUserId === userId) {
        myPaid += expense.amount
      }
      
      // Kiek aš turiu sumokėti (mano dalis)
      const myShareInExpense = expense.shares?.find(s => s.userId === userId)
      if (myShareInExpense) {
        myShare += myShareInExpense.shareAmount
      }
    })
    
    const totalAmount = getTotalExpenses.value(activityId, taskId)
    const balance = myPaid - myShare // positive = permoka, negative = skola
    
    return {
      totalAmount,
      myShare,
      myPaid,
      balance,
      expenses
    }
  })

  // Actions
  async function fetchExpenses(activityId: number, taskId: number, forceRefresh = false) {
    const key = getKey(activityId, taskId)
    
    if (expensesByTask.value[key] && !forceRefresh) {
      return expensesByTask.value[key]
    }

    loading.value = true
    error.value = null
    
    try {
      const response = await expensesAPI.getAll(activityId, taskId)
      
      if (response.ok && response.data) {
        expensesByTask.value[key] = response.data
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko gauti išlaidų'
        return []
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant išlaidas'
      console.error('Fetch expenses error:', e)
      return []
    } finally {
      loading.value = false
    }
  }

  async function fetchExpense(activityId: number, taskId: number, expenseId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await expensesAPI.getById(activityId, taskId, expenseId)
      
      if (response.ok && response.data) {
        currentExpense.value = response.data
        
        // Atnaujinti cache'e
        const key = getKey(activityId, taskId)
        if (expensesByTask.value[key]) {
          const index = expensesByTask.value[key].findIndex(e => e.id === expenseId)
          if (index !== -1) {
            expensesByTask.value[key][index] = response.data
          }
        }
        
        return response.data
      } else {
        error.value = response.error?.message || 'Išlaida nerasta'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida gaunant išlaidas'
      console.error('Fetch expense error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function createExpense(activityId: number, taskId: number, data: CreateExpenseRequest) {
    loading.value = true
    error.value = null
    
    try {
      const payload: CreateExpenseRequest = {
        ...data,
        expenseDate: toUtcIso((data as any).expenseDate) || new Date().toISOString()
      }
      const response = await expensesAPI.create(activityId, taskId, payload)
      
      if (response.ok && response.data) {
        const key = getKey(activityId, taskId)
        
        // Pridėti į cache
        if (!expensesByTask.value[key]) {
          expensesByTask.value[key] = []
        }
        expensesByTask.value[key].push(response.data)
        // Persist last paidBy
        lastPaidByUserId.value = response.data.paidByUserId
        
        return response.data
      } else {
        error.value = response.error?.message || 'Nepavyko sukurti išlaidos'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida kuriant išlaidas'
      console.error('Create expense error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateExpense(
    activityId: number, 
    taskId: number, 
    expenseId: number, 
    data: UpdateExpenseRequest
  ) {
    loading.value = true
    error.value = null
    
    try {
      const payload: UpdateExpenseRequest = {
        ...data,
        expenseDate: (data as any).expenseDate ? (toUtcIso((data as any).expenseDate) || undefined) : undefined
      }
      const response = await expensesAPI.update(activityId, taskId, expenseId, payload)
      
      // Handle 204 No Content response (successful update)
      if (response.ok) {
        // Refetch to get updated data
        const refetchResponse = await expensesAPI.getById(activityId, taskId, expenseId)
        
        if (refetchResponse.ok && refetchResponse.data) {
          const key = getKey(activityId, taskId)
          
          // Update cache
          if (expensesByTask.value[key]) {
            const index = expensesByTask.value[key].findIndex(e => e.id === expenseId)
            if (index !== -1) {
              expensesByTask.value[key][index] = refetchResponse.data
            }
          }
          
          // Update current
          if (currentExpense.value?.id === expenseId) {
            currentExpense.value = refetchResponse.data
          }
          // Update last paidBy
          lastPaidByUserId.value = refetchResponse.data.paidByUserId
          
          return refetchResponse.data
        }
        
        return null
      } else {
        error.value = response.error?.message || 'Nepavyko atnaujinti išlaidos'
        return null
      }
    } catch (e) {
      error.value = 'Netikėta klaida atnaujinant išlaidas'
      console.error('Update expense error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  async function deleteExpense(activityId: number, taskId: number, expenseId: number) {
    loading.value = true
    error.value = null
    
    try {
      const response = await expensesAPI.delete(activityId, taskId, expenseId)
      
      if (response.ok) {
        const key = getKey(activityId, taskId)
        
        // Pašalinti iš cache
        if (expensesByTask.value[key]) {
          expensesByTask.value[key] = expensesByTask.value[key].filter(
            e => e.id !== expenseId
          )
        }
        
        // Išvalyti current
        if (currentExpense.value?.id === expenseId) {
          currentExpense.value = null
        }
        
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko ištrinti išlaidos'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida trinant išlaidas'
      console.error('Delete expense error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentExpense() {
    currentExpense.value = null
  }

  function clearExpensesForTask(activityId: number, taskId: number) {
    const key = getKey(activityId, taskId)
    delete expensesByTask.value[key]
  }

  function reset() {
    expensesByTask.value = {}
    currentExpense.value = null
    loading.value = false
    error.value = null
    lastPaidByUserId.value = null
  }

  async function markShareAsPaid(
    activityId: number, 
    taskId: number, 
    expenseId: number, 
    shareId: number
  ) {
    loading.value = true
    error.value = null
    
    try {
      const response = await expensesAPI.markShareAsPaid(activityId, taskId, expenseId, shareId)
      
      if (response.ok) {
        // Refresh expense data
        await fetchExpenses(activityId, taskId, true)
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko patvirtinti mokėjimo'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida patvirtinant mokėjimą'
      console.error('Mark share paid error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function unmarkShareAsPaid(
    activityId: number, 
    taskId: number, 
    expenseId: number, 
    shareId: number
  ) {
    loading.value = true
    error.value = null
    
    try {
      const response = await expensesAPI.unmarkShareAsPaid(activityId, taskId, expenseId, shareId)
      
      if (response.ok) {
        // Refresh expense data
        await fetchExpenses(activityId, taskId, true)
        return true
      } else {
        error.value = response.error?.message || 'Nepavyko atšaukti mokėjimo patvirtinimo'
        return false
      }
    } catch (e) {
      error.value = 'Netikėta klaida atšaukiant mokėjimą'
      console.error('Unmark share paid error:', e)
      return false
    } finally {
      loading.value = false
    }
  }

  return {
    // State
    expensesByTask,
    currentExpense,
    loading,
    error,
    lastPaidByUserId,
    
    // Getters
    getExpenses,
    getTotalExpenses,
    getExpenseSummary,
    
    // Actions
    fetchExpenses,
    fetchExpense,
    createExpense,
    updateExpense,
    deleteExpense,
    markShareAsPaid,
    unmarkShareAsPaid,
    clearError,
    clearCurrentExpense,
    clearExpensesForTask,
    reset
  }
})
