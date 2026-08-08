import { api } from './client'
import type {
  LoanType,
  LoanPermissionRequestItem,
  NotificationItem,
  MyLoanItem,
  UpdateProfilePayload,
} from './types'

// ---------- Profile ----------
export async function updateProfile(payload: UpdateProfilePayload) {
  const res = await api.put('/auth/profile', payload)
  return res.data as { message: string }
}

export async function uploadProfilePicture(file: File) {
  const form = new FormData()
  form.append('file', file)
  const res = await api.post('/auth/profile/picture', form)
  return res.data as { profilePictureUrl: string; message: string }
}

export async function deleteProfilePicture() {
  const res = await api.delete('/auth/profile/picture')
  return res.data as { message: string }
}

// ---------- My loans (history) ----------
export async function getMyLoans(): Promise<MyLoanItem[]> {
  const res = await api.get<MyLoanItem[]>('/loan/my-loans')
  return res.data
}

// ---------- Loan types ----------
export async function getLoanTypes(activeOnly = false): Promise<LoanType[]> {
  const res = await api.get<{ items: LoanType[] }>('/loantype', {
    params: { activeOnly },
  })
  return res.data.items
}

export async function setLoanTypeStatus(id: number, isActive: boolean) {
  const res = await api.put(`/loantype/${id}/status`, { isActive })
  return res.data
}

// ---------- Loan permission requests ----------
export async function createPermissionRequest(loanTypeId: number, reason: string) {
  const res = await api.post('/loanpermission/request', { loanTypeId, reason })
  return res.data
}

export async function getMyPermissionRequests(): Promise<LoanPermissionRequestItem[]> {
  const res = await api.get<{ items: LoanPermissionRequestItem[] }>('/loanpermission/my')
  return res.data.items
}

export async function getAllPermissionRequests(): Promise<LoanPermissionRequestItem[]> {
  const res = await api.get<{ items: LoanPermissionRequestItem[] }>('/loanpermission/all', {
    params: { page: 1, pageSize: 100 },
  })
  return res.data.items
}

export async function approvePermissionRequest(id: string) {
  const res = await api.post(`/loanpermission/${id}/approve`)
  return res.data
}

export async function rejectPermissionRequest(id: string, adminResponse?: string) {
  const res = await api.post(`/loanpermission/${id}/reject`, { adminResponse })
  return res.data
}

// ---------- Notifications ----------
export async function getMyNotifications(): Promise<{
  items: NotificationItem[]
  unreadCount: number
}> {
  const res = await api.get('/notification/my')
  return res.data
}

export async function getUnreadCount(): Promise<number> {
  const res = await api.get<{ unreadCount: number }>('/notification/unread-count')
  return res.data.unreadCount
}

export async function markAllNotificationsRead() {
  const res = await api.post('/notification/read-all')
  return res.data
}

// ---------- Employees (admin) ----------
export interface CreateEmployeePayload {
  firstName: string
  lastName: string
  personnelNumber: string
  username: string
  hireDate: string
  role?: string
}

export async function createEmployee(payload: CreateEmployeePayload) {
  const res = await api.post('/employee', payload)
  return res.data as {
    id: string
    username: string
    temporaryPassword: string
    message: string
  }
}

export async function getAllEmployees() {
  const res = await api.get('/employee')
  return res.data
}
