// تایپ‌های مشترک که با DTOهای بک‌اند مطابق‌اند.

export interface SessionInfo {
  id: number
  deviceOs?: string | null
  deviceBrowser?: string | null
  ipAddress?: string | null
  lastSeenAt: string
  createdAt: string
  isCurrent: boolean
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expireAt: string
  /** اگر true باشد، کاربر به سقفِ نشست‌ها رسیده و باید یکی را قطع کند. */
  requiresSessionChoice?: boolean
  sessions?: SessionInfo[]
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
  /** برای ادمین‌ها: ارشد است یا ادمین وام. فرانت بر این اساس داشبورد را انتخاب می‌کند. */
  isSeniorAdmin: boolean
  /** برای «ادمین وام»: شناسه و نام وامی که مدیریت می‌کند. */
  managedLoanTypeId?: number | null
  managedLoanTypeName?: string | null
  profilePictureUrl?: string | null
  jobPositionTitle?: string | null
  effectiveMonthlySalary: number
  /** سقف قسط ماهانه؛ فرم درخواست وام سقف مبلغ را از روی همین حساب می‌کند. */
  maxMonthlyInstallment: number
  employmentStatus: 'Active' | 'Terminated'
  minimumScoreRequiredForLoan: number
  /** مجوز استثنایی یک‌بارمصرف که ادمین داده. */
  hasLoanPermission: boolean
  /**
   * آیا کارمند در این لحظه می‌تواند درخواست وام بدهد.
   * سرور تصمیم می‌گیرد — امتیاز کافی یا مجوز استثنایی، هر کدام کافی است.
   */
  canRequestLoan: boolean
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
  requiresDocument: boolean
  requiredDocumentDescription?: string | null
  hasDocument: boolean
  totalInstallments: number
  paidInstallments: number
  createdAt: string
}

/** یک گزینه‌ی تعداد قسط با مبلغ ماهانه‌ی محاسبه‌شده در سرور. */
export interface InstallmentOption {
  installmentCount: number
  monthlyPayment: number
  totalPayable: number
  totalFee: number
  /** آیا قسط از سقف حقوق کارمند عبور نمی‌کند. */
  isAffordable: boolean
}

/** آنچه فرم درخواست وام برای پر کردن لیست‌هایش لازم دارد. */
export interface LoanQuote {
  loanTypeId: number
  loanTypeName: string
  isEligible: boolean
  ineligibilityReason?: string | null
  minAmount: number
  maxAmount: number
  amountStep: number
  amountOptions: number[]
  annualFeePercent: number
  requiresDocument: boolean
  requiredDocumentDescription?: string | null
  maxMonthlyInstallment: number
  /** تاریخ عقد ثبت‌شده در پروفایل، اگر باشد. فرم ازدواج بر همین اساس تصمیم می‌گیرد بپرسد یا فقط نشان دهد. */
  marriageDate?: string | null
  marriageDatePersian?: string | null
  installmentOptions: InstallmentOption[]
}

/** مدرک پیوست‌شده به یک وام. */
export interface LoanDocumentItem {
  id: string
  fileName: string
  filePath: string
  uploadedAt: string
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
