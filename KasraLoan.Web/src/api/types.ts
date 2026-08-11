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
  additionalPhoneNumbers: string[]
  email?: string | null
  score: number
  role: 'Admin' | 'Employee'
  profilePictureUrl?: string | null
  jobPositionTitle?: string | null
  effectiveMonthlySalary: number
  /** سقف قسط ماهانه؛ فرم درخواست وام سقف مبلغ را از روی همین حساب می‌کند. */
  maxMonthlyInstallment: number
  employmentStatus: 'Active' | 'Terminated'
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

export interface MyLoanItem {
  id: string
  loanType: string
  requestedAmount: number
  approvedAmount: number
  installmentCount: number
  status: string
  totalPayableAmount: number
  monthlyPaymentAmount: number
  createdAt: string
}

/** یک وام در جدول ادمین. */
export interface AdminLoanItem {
  id: string
  employeeId: string
  employeeName: string
  employeeUsername: string
  loanTypeId: number
  loanTypeName: string
  requestedAmount: number
  approvedAmount: number
  installmentCount: number
  status: string
  totalPayableAmount: number
  monthlyPaymentAmount: number
  annualFeePercent: number
  createdAt: string
}

export interface LoanInstallment {
  id: string
  installmentNumber: number
  amount: number
  dueDate: string
  isPaid: boolean
}

export type PaymentMethod = 'PayrollDeduction' | 'OnlineGateway' | 'Cheque'

export type PaymentStatus =
  | 'Selected'
  | 'AwaitingAdminApproval'
  | 'Confirmed'
  | 'Rejected'
  | 'Failed'

/** قسط بعدی و وضعیت پنجره‌ی انتخاب روش پرداخت. */
export interface CurrentInstallment {
  hasDueInstallment: boolean
  loanInstallmentId?: string | null
  installmentNumber: number
  amount: number
  dueDate?: string | null
  dueDatePersian?: string | null
  isSelectionWindowOpen: boolean
  selectedMethod?: PaymentMethod | null
  paymentStatus?: PaymentStatus | null
  windowDescription: string
}

/** یک تلاش پرداخت. */
export interface InstallmentPaymentItem {
  id: string
  loanInstallmentId: string
  installmentNumber: number
  method: PaymentMethod
  status: PaymentStatus
  amount: number
  chequeImageUrl?: string | null
  chequeNumber?: string | null
  chequeBankName?: string | null
  chequeDate?: string | null
  chequeDatePersian?: string | null
  gatewayRefId?: string | null
  rejectReason?: string | null
  createdAt: string
  employeeName?: string | null
  loanTypeName?: string | null
}

/** نشستِ پرداخت آنلاین. */
export interface GatewaySession {
  authority: string
  amount: number
  installmentNumber: number
  gatewayName: string
  expiresAt: string
  redirectUrl: string
}

/** مانده‌ی وام و وضعیت مطالبه‌ی تسویه‌ی یکجا. */
export interface LoanOutstanding {
  loanRequestId: string
  totalPayableAmount: number
  paidAmount: number
  outstandingAmount: number
  totalInstallments: number
  paidInstallments: number
  remainingInstallments: number
  isSettlementDemanded: boolean
  settlementDueDate?: string | null
  settlementDueDatePersian?: string | null
  settlementReason?: string | null
}

export interface UpdateProfilePayload {
  newPassword?: string
  phoneNumber?: string
  additionalPhoneNumbers?: string[]
  email?: string
}
