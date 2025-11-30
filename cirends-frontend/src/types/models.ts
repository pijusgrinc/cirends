import type { Role, TaskStatus, InvitationStatus } from './enums'

/**
 * Vartotojas
 */
export interface User {
  id: number
  name: string
  email: string
  role: Role | string
  createdAt: string
}

/**
 * Veikla - pagrindinis hierarchijos objektas
 * Veikla apima laiką, vietą, aprašymą ir gali turėti dalyvių
 */
export interface Activity {
  id: number
  name: string
  description: string
  startDate: string
  endDate: string | null
  location: string | null
  createdByUserId: number
  createdAt: string
  createdBy?: User
  participants?: ActivityUser[]
  tasks?: Task[]
  totalExpenses?: number
}

/**
 * Veiklos dalyvis - junction table tarp Activity ir User
 */
export interface ActivityUser {
  activityId: number
  userId: number
  isAdmin: boolean
  joinedAt: string
  user?: User
}

/**
 * Užduotis - priskiriama veiklai
 * Turi atsakingą asmenį ir gali turėti išlaidų
 */
export interface Task {
  id: number
  name: string
  description: string | null
  status: TaskStatus
  assignedUserId: number | null
  activityId: number
  dueDate: string | null
  createdAt: string
  assignedUser?: User
  activity?: Activity
  expenses?: Expense[]
  totalExpenses?: number
}

/**
 * Išlaidos - priskiriama užduočiai arba veiklai
 * Leidžia sekti finansus ir paskirstyti tarp dalyvių
 */
export interface Expense {
  id: number
  name: string
  amount: number
  taskId: number
  expenseDate: string
  currency?: string
  paidByUserId: number
  createdAt: string
  paidBy?: User
  task?: Task
  shares?: ExpenseShare[]
  expenseShares?: ExpenseShare[]  // API grąžina su šiuo vardu
}

/**
 * Išlaidų paskirstymas - kaip išlaidos dalijamos tarp dalyvių
 */
export interface ExpenseShare {
  id: number
  expenseId: number
  userId: number
  shareAmount: number
  sharePercentage?: number
  isPaid: boolean
  paidAt: string | null
  user?: User
}

/**
 * Kvietimas į veiklą
 */
export interface Invitation {
  id: number
  activityId: number
  activityName?: string
  invitedUserId: number
  status: InvitationStatus
  createdAt: string
  respondedAt?: string | null
  activity?: Activity
  invitedUser?: User
  invitedBy?: User
}

/**
 * Statistika dashboard'ui
 */
export interface DashboardStats {
  activitiesCount: number
  tasksCount: number
  completedTasksCount: number
  totalExpenses: number
  myDebt: number
  myOverpayment: number
  pendingInvitationsCount: number
}

/**
 * Išlaidų suvestinė
 */
export interface ExpenseSummary {
  totalAmount: number
  myShare: number
  myPaid: number
  balance: number // negative = skola, positive = permoka
  expenses: Expense[]
}
