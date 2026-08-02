// تایپ‌های مشترک که با DTOهای بک‌اند مطابق‌اند.

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expireAt: string
}

export interface CurrentUser {
  id: string
  firstName: string
  lastName: string
  username: string
  personnelNumber: string
  phoneNumber?: string | null
  email?: string | null
  score: number
  role: 'Admin' | 'Employee'
}

export interface LoanType {
  id: number
  name: string
  type: string
  isActive: boolean
}

export interface LoanPermissionRequestItem {
  id: string
  employeeId: string
  employeeName: string
  employeeUsername: string
  loanTypeId: number
  loanTypeName: string
  reason: string
  status: 'Pending' | 'Approved' | 'Rejected'
  createdAt: string
  reviewedAt?: string | null
  adminResponse?: string | null
}

export interface NotificationItem {
  id: string
  title: string
  message: string
  isRead: boolean
  createdAt: string
}
