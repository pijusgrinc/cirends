/**
 * Type declarations for api.js
 * This file provides TypeScript types for the JavaScript API module
 */

import type { ApiResponse, LoginResponse } from './types'
import type { User, Activity, Task, Expense, Invitation } from './types'

// Auth API
export const authAPI: {
  register(email: string, password: string, name: string): Promise<ApiResponse<LoginResponse>>
  login(email: string, password: string): Promise<ApiResponse<LoginResponse>>
  logout(): Promise<ApiResponse<void>>
  refresh(): Promise<ApiResponse<LoginResponse>>
}

// Users API
export const usersAPI: {
  getProfile(): Promise<ApiResponse<User>>
  getAllUsers(): Promise<ApiResponse<User[]>>
  getUserById(id: number): Promise<ApiResponse<User>>
  updateUser(id: number, data: any): Promise<ApiResponse<User>>
  deleteUser(id: number): Promise<ApiResponse<void>>
}

// Activities API
export const activitiesAPI: {
  getAll(): Promise<ApiResponse<Activity[]>>
  getById(id: number): Promise<ApiResponse<Activity>>
  create(data: any): Promise<ApiResponse<Activity>>
  update(id: number, data: any): Promise<ApiResponse<Activity>>
  delete(id: number): Promise<ApiResponse<void>>
  getParticipants(id: number): Promise<ApiResponse<User[]>>
  addParticipant(id: number, userId: number): Promise<ApiResponse<void>>
  removeParticipant(id: number, userId: number): Promise<ApiResponse<void>>
}

// Tasks API
export const tasksAPI: {
  getAll(activityId: number): Promise<ApiResponse<Task[]>>
  getById(activityId: number, taskId: number): Promise<ApiResponse<Task>>
  create(activityId: number, data: any): Promise<ApiResponse<Task>>
  update(activityId: number, taskId: number, data: any): Promise<ApiResponse<Task>>
  delete(activityId: number, taskId: number): Promise<ApiResponse<void>>
}

// Expenses API
export const expensesAPI: {
  getAll(activityId: number, taskId: number): Promise<ApiResponse<Expense[]>>
  getById(activityId: number, taskId: number, expenseId: number): Promise<ApiResponse<Expense>>
  create(activityId: number, taskId: number, data: any): Promise<ApiResponse<Expense>>
  update(activityId: number, taskId: number, expenseId: number, data: any): Promise<ApiResponse<Expense>>
  delete(activityId: number, taskId: number, expenseId: number): Promise<ApiResponse<void>>
  markShareAsPaid(activityId: number, taskId: number, expenseId: number, shareId: number): Promise<ApiResponse<void>>
  unmarkShareAsPaid(activityId: number, taskId: number, expenseId: number, shareId: number): Promise<ApiResponse<void>>
  markAllExpensesAsPaidForTask(activityId: number, taskId: number): Promise<ApiResponse<void>>
}

// Invitations API
export const invitationsAPI: {
  getAll(): Promise<ApiResponse<Invitation[]>>
  getPending(): Promise<ApiResponse<Invitation[]>>
  getActivityInvitations(activityId: number): Promise<ApiResponse<Invitation[]>>
  send(email: string, activityId: number): Promise<ApiResponse<Invitation>>
  accept(invitationId: number): Promise<ApiResponse<void>>
  reject(invitationId: number): Promise<ApiResponse<void>>
}
