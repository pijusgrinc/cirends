import { ref } from 'vue'
import type { ApiResponse } from '@/types'

/**
 * Composable API užklausoms su loading, error handling ir data management
 * 
 * @example
 * const { data, loading, error, execute } = useApi<Activity[]>()
 * await execute(() => activitiesAPI.getAll())
 */
export function useApi<T = any>() {
  const data = ref<T | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function execute(apiCall: () => Promise<ApiResponse<T>>): Promise<T | null> {
    loading.value = true
    error.value = null
    
    try {
      const response = await apiCall()
      
      if (response.ok && response.data !== null) {
        data.value = response.data
        return response.data
      } else {
        const errorMessage = typeof response.error === 'string' 
          ? response.error 
          : response.error?.message || 'Įvyko klaida'
        
        error.value = errorMessage
        return null
      }
    } catch (e) {
      const errorMessage = e instanceof Error ? e.message : 'Netikėta klaida'
      error.value = errorMessage
      console.error('API error:', e)
      return null
    } finally {
      loading.value = false
    }
  }

  function reset() {
    data.value = null
    error.value = null
    loading.value = false
  }

  function clearError() {
    error.value = null
  }

  return {
    data,
    loading,
    error,
    execute,
    reset,
    clearError
  }
}
