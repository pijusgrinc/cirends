import type { 
  User, 
  Activity, 
  Task, 
  Expense, 
  ExpenseShare, 
  Invitation,
  DashboardStats 
} from './models'

/**
 * Bendras API atsakymo tipas
 */
export interface ApiResponse<T> {
  ok: boolean
  status: number
  data: T | null
  error: any
}

// ============== AUTH ==============

export interface RegisterRequest {
  email: string
  password: string
  name: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  user: User
}

// ============== ACTIVITIES ==============

export interface CreateActivityRequest {
  name: string
  description: string
  startDate: string
  endDate?: string | null
  location?: string | null
}

export interface UpdateActivityRequest {
  name?: string
  description?: string
  startDate?: string
  endDate?: string | null
  location?: string | null
}

export interface ActivityParticipant {
  userId: number
  user?: User
}

// ============== TASKS ==============

export interface CreateTaskRequest {
  name: string
  description?: string | null
  assignedUserId?: number | null
  dueDate?: string | null
  status?: number
  priority?: number
}

export interface UpdateTaskRequest {
  name?: string
  description?: string | null
  assignedUserId?: number | null
  dueDate?: string | null
  status?: number
  priority?: number
}

// ============== EXPENSES ==============

export interface CreateExpenseRequest {
  name: string
  amount: number
  currency?: string
  expenseDate: string
  shares: ExpenseShareInput[]
}

export interface UpdateExpenseRequest {
  name?: string
  amount?: number
  currency?: string
  expenseDate?: string
  shares?: ExpenseShareInput[]
}

export interface ExpenseShareInput {
  userId: number
  shareAmount: number
}

// ============== INVITATIONS ==============

export interface CreateInvitationRequest {
  activityId: number
  invitedUserId: number
}

export interface RespondToInvitationRequest {
  accept: boolean
}

// ============== USERS ==============

export interface UpdateUserRequest {
  name?: string
  email?: string
}

// ============== PAGINATION ==============

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface PaginationParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}
