/**
 * Vartotojo rolės sistemoje
 */
export enum Role {
  Guest = 0,
  Member = 1,
  Admin = 2
}

/**
 * Užduoties būsenos
 */
export enum TaskStatus {
  Planned = 0,
  InProgress = 1,
  Completed = 2
}

/**
 * Kvietimo būsenos
 */
export enum InvitationStatus {
  Pending = 0,
  Accepted = 1,
  Rejected = 2
}

/**
 * Užduoties būsenų pavadinimai lietuviškai
 */
export const TaskStatusLabels: Record<TaskStatus, string> = {
  [TaskStatus.Planned]: 'Planuojama',
  [TaskStatus.InProgress]: 'Vykdoma',
  [TaskStatus.Completed]: 'Atlikta'
}

/**
 * Kvietimo būsenų pavadinimai lietuviškai
 */
export const InvitationStatusLabels: Record<InvitationStatus, string> = {
  [InvitationStatus.Pending]: 'Laukiama',
  [InvitationStatus.Accepted]: 'Priimta',
  [InvitationStatus.Rejected]: 'Atmesta'
}

/**
 * Rolių pavadinimai lietuviškai
 */
export const RoleLabels: Record<Role, string> = {
  [Role.Guest]: 'Svečias',
  [Role.Member]: 'Narys',
  [Role.Admin]: 'Administratorius'
}

/**
 * Helper funkcijos labelų gavimui
 */
export function getRoleLabel(role: Role): string {
  return RoleLabels[role]
}

export function getTaskStatusLabel(status: TaskStatus): string {
  return TaskStatusLabels[status]
}

export function getInvitationStatusLabel(status: InvitationStatus): string {
  return InvitationStatusLabels[status]
}
