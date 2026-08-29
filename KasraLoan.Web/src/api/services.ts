import { api } from './client'
import type {
  SessionInfo,
  LoginHistoryItem,
  LoanType,
  LoanPermissionRequestItem,
  NotificationItem,
  MyLoanItem,
  AdminLoanItem,
  LoanInstallment,
  LoanOutstanding,
  UpdateProfilePayload,
  CurrentInstallment,
  InstallmentPaymentItem,
  GatewaySession,
  PaymentMethod,
  LoanDocumentItem,
  LoanQuote,
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

// ---------- Loan requests ----------
/**
 * سقف، گزینه‌های مبلغ و — اگر مبلغ بدهی — گزینه‌های تعداد اقساط با مبلغ ماهانه.
 * همه‌ی محاسبات سمت سرور است تا فرم فرمول کارمزد را تکرار نکند.
 */
export async function getLoanQuote(loanTypeId: number, amount?: number): Promise<LoanQuote> {
  const res = await api.get<LoanQuote>('/loan/quote', {
    params: { loanTypeId, amount },
  })
  return res.data
}

/**
 * ثبت درخواست وام همراه مدارک، در یک درخواست multipart.
 * برای وام‌هایی که مدرک لازم دارند، سرور بدون فایل درخواست را نمی‌سازد.
 */
export async function createLoanRequest(payload: {
  loanTypeId: number
  requestedAmount: number
  installmentCount: number
  destinationType?: string
  destination?: string
  startDate?: string
  endDate?: string
  marriageDate?: string
  spouseFirstName?: string
  spouseLastName?: string
  spouseNationalId?: string
  specialCaseCategory?: string
  specialCaseDescription?: string
  immediatePaymentPurpose?: string
  notes?: string
  files?: File[]
}) {
  const form = new FormData()
  form.append('LoanTypeId', String(payload.loanTypeId))
  form.append('RequestedAmount', String(payload.requestedAmount))
  form.append('InstallmentCount', String(payload.installmentCount))

  if (payload.destinationType) form.append('DestinationType', payload.destinationType)
  if (payload.destination) form.append('Destination', payload.destination)
  if (payload.startDate) form.append('StartDate', payload.startDate)
  if (payload.endDate) form.append('EndDate', payload.endDate)

  if (payload.marriageDate) form.append('MarriageDate', payload.marriageDate)
  if (payload.spouseFirstName) form.append('SpouseFirstName', payload.spouseFirstName)
  if (payload.spouseLastName) form.append('SpouseLastName', payload.spouseLastName)
  if (payload.spouseNationalId) form.append('SpouseNationalId', payload.spouseNationalId)
  if (payload.specialCaseCategory) form.append('SpecialCaseCategory', payload.specialCaseCategory)
  if (payload.specialCaseDescription)
    form.append('SpecialCaseDescription', payload.specialCaseDescription)
  if (payload.immediatePaymentPurpose)
    form.append('ImmediatePaymentPurpose', payload.immediatePaymentPurpose)

  if (payload.notes) form.append('Notes', payload.notes)

  for (const file of payload.files ?? []) form.append('Files', file)

  const res = await api.post('/loan/request', form)
  return res.data as {
    loanRequestId: string
    message: string
    requiresDocument: boolean
    requiredDocumentDescription?: string | null
  }
}

/**
 * بارگذاری مدرک وام.
 * اندپوینت به شناسه‌ی وام نیاز دارد، پس فقط بعد از ثبت درخواست قابل فراخوانی است.
 */
export async function uploadLoanDocument(loanId: string, file: File) {
  const form = new FormData()
  form.append('file', file)

  const res = await api.post(`/loan/${loanId}/upload-document`, form)
  return res.data as { isSuccess: boolean; message: string }
}

export async function getLoanDocuments(loanId: string): Promise<LoanDocumentItem[]> {
  const res = await api.get<LoanDocumentItem[]>(`/loan/${loanId}/documents`)
  return res.data
}

/** لیست همه‌ی وام‌ها برای ادمین. */
export async function getAllLoans(status?: string): Promise<AdminLoanItem[]> {
  const res = await api.get<{ items: AdminLoanItem[] }>('/loan/all', {
    params: { page: 1, pageSize: 100, status },
  })
  return res.data.items
}

export async function approveLoan(id: string) {
  const res = await api.post(`/loan/approve/${id}`)
  return res.data as { message: string }
}

export async function rejectLoan(id: string, rejectReason?: string) {
  const res = await api.post(`/loan/reject/${id}`, { rejectReason })
  return res.data as { message: string }
}

// ---------- Installments ----------
export async function getLoanInstallments(loanId: string): Promise<LoanInstallment[]> {
  const res = await api.get<{ data: LoanInstallment[] }>(`/loan/${loanId}/installments`)
  return res.data.data ?? []
}

export async function getLoanOutstanding(loanId: string): Promise<LoanOutstanding> {
  const res = await api.get<LoanOutstanding>(`/loan/${loanId}/outstanding`)
  return res.data
}

/** پرداخت قسط. فقط صاحب وام مجاز است، نه ادمین. */
export async function payInstallment(installmentId: string) {
  const res = await api.post(`/loan/installments/${installmentId}/pay`)
  return res.data as { message: string }
}

// ---------- Installment payments ----------
export async function getCurrentInstallment(): Promise<CurrentInstallment> {
  const res = await api.get<CurrentInstallment>('/installmentpayment/current')
  return res.data
}

export async function selectPaymentMethod(installmentId: string, method: PaymentMethod) {
  const res = await api.post(`/installmentpayment/${installmentId}/method`, { method })
  return res.data as InstallmentPaymentItem
}

export async function submitCheque(
  installmentId: string,
  info: { chequeNumber: string; chequeBankName: string; chequeDate: string },
  file: File,
) {
  const form = new FormData()
  form.append('file', file)
  form.append('ChequeNumber', info.chequeNumber)
  form.append('ChequeBankName', info.chequeBankName)
  form.append('ChequeDate', info.chequeDate)

  const res = await api.post(`/installmentpayment/${installmentId}/cheque`, form)
  return res.data as InstallmentPaymentItem
}

export async function startGatewayPayment(installmentId: string): Promise<GatewaySession> {
  const res = await api.post<GatewaySession>(`/installmentpayment/${installmentId}/gateway`)
  return res.data
}

export async function getGatewaySession(authority: string): Promise<GatewaySession> {
  const res = await api.get<GatewaySession>(`/installmentpayment/gateway/${authority}`)
  return res.data
}

/** اطلاعات کارت فقط ارسال می‌شود و هیچ‌جا در فرانت نگه داشته نمی‌شود. */
export async function payViaGateway(
  authority: string,
  card: {
    cardNumber: string
    cvv2: string
    expiryMonth: string
    expiryYear: string
    secondPassword: string
  },
) {
  const res = await api.post(`/installmentpayment/gateway/${authority}/pay`, card)
  return res.data as InstallmentPaymentItem
}

export async function getPendingCheques(): Promise<InstallmentPaymentItem[]> {
  const res = await api.get<InstallmentPaymentItem[]>('/installmentpayment/cheques/pending')
  return res.data
}

export async function confirmCheque(paymentId: string) {
  const res = await api.post(`/installmentpayment/cheques/${paymentId}/confirm`)
  return res.data as InstallmentPaymentItem
}

export async function rejectCheque(paymentId: string, rejectReason: string) {
  const res = await api.post(`/installmentpayment/cheques/${paymentId}/reject`, { rejectReason })
  return res.data as InstallmentPaymentItem
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
  /** کد ملی (دقیقاً ۱۰ رقم). الزامی. */
  nationalId: string
  /** رمزی که ادمین برای کاربر تعیین می‌کند. */
  password: string
  /** فقط برای ادمین لازم است؛ برای کارمند خودکار (برابر نام کاربری) ساخته می‌شود. */
  personnelNumber?: string
  /** فقط برای ادمین لازم است؛ نام کاربری کارمند سمت سرور خودکار ساخته می‌شود. */
  username?: string
  hireDate: string
  role?: string
  /** برای نقش Employee الزامی است؛ حقوق و سقف وام از روی آن حساب می‌شود. */
  jobPositionId?: number
  /** حقوق اختصاصی؛ اگر خالی باشد حقوق پایه‌ی سمت استفاده می‌شود. */
  monthlySalary?: number
  /** برای نقش Admin: ادمین ارشد است یا ادمین وام. */
  isSeniorAdmin?: boolean
  /** برای «ادمین وام»: شناسه‌ی وامی که مدیریت می‌کند. */
  managedLoanTypeId?: number
}

// ---------- Request pool (all employee requests, for senior admins) ----------
export interface RequestPoolItem {
  id: string
  category: 'Loan' | 'Permission'
  categoryLabel: string
  loanTypeId: number
  loanTypeName: string
  employeeName: string
  employeeUsername: string
  status: string
  createdAt: string
  detail?: string | null
}

/** استخرِ همه‌ی درخواست‌های کارمندان (وام + مجوز وام) — فقط ادمین ارشد. */
export async function getRequestPool(): Promise<RequestPoolItem[]> {
  const res = await api.get<{ items: RequestPoolItem[] }>('/loan/requests/pool')
  return res.data.items
}

// ---------- Job positions ----------
export interface JobPosition {
  id: number
  title: string
  baseSalary: number
  isActive: boolean
  employeeCount: number
}

export async function getJobPositions(activeOnly = false): Promise<JobPosition[]> {
  const res = await api.get<{ items: JobPosition[] }>('/jobposition', {
    params: { activeOnly },
  })
  return res.data.items
}

/** کد ۹ رقمیِ بعدی (نام کاربری = شماره‌ی پرسنلی) را برای پیش‌نمایش در فرم برمی‌گرداند. */
export async function getNextIdentifier(
  jobPositionId: number,
  hireDate: string,
): Promise<string> {
  const res = await api.get<{ identifier: string }>('/employee/next-identifier', {
    params: { jobPositionId, hireDate },
  })
  return res.data.identifier
}

export async function createEmployee(payload: CreateEmployeePayload) {
  const res = await api.post('/employee', payload)
  return res.data as {
    id: string
    username: string
    message: string
  }
}

export async function getAllEmployees() {
  const res = await api.get('/employee')
  return res.data
}

/** تغییر رمز عبورِ کاربرِ جاری؛ رمز فعلی سمت سرور تأیید می‌شود. */
export async function changePassword(currentPassword: string, newPassword: string) {
  const res = await api.post('/auth/change-password', { currentPassword, newPassword })
  return res.data as { message: string }
}

// ---------- Forgot / reset password ----------
export interface ForgotPasswordResult {
  message: string
  emailSent: boolean
  /** فقط در حالتِ تست (SMTP خاموش): رمزِ موقت برای نمایش. در حالتِ واقعی null است. */
  devTempPassword?: string | null
}

/** درخواستِ رمزِ موقت با ایمیل. اگر ایمیل ثبت نشده باشد، سرور خطای ۴۰۰ می‌دهد. */
export async function forgotPassword(email: string): Promise<ForgotPasswordResult> {
  const res = await api.post('/auth/forgot-password', { email })
  return res.data as ForgotPasswordResult
}

/** تعیینِ رمزِ جدید بعد از ورود با رمزِ موقت (بدونِ رمزِ فعلی). */
export async function resetPassword(newPassword: string) {
  const res = await api.post('/auth/reset-password', { newPassword })
  return res.data as { message: string }
}

// ---------- Forgot password via national ID ----------
/** فراموشی رمز — مرحله ۱: تأیید نام کاربری + کد ملی (خطای ۴۰۰ اگر نادرست باشد). */
export async function verifyIdentity(username: string, nationalId: string) {
  const res = await api.post('/auth/verify-identity', { username, nationalId })
  return res.data as { message: string }
}

/** فراموشی رمز — مرحله ۲: تعیین رمز جدید پس از تأیید نام کاربری + کد ملی. */
export async function resetByIdentity(username: string, nationalId: string, newPassword: string) {
  const res = await api.post('/auth/reset-by-identity', { username, nationalId, newPassword })
  return res.data as { message: string }
}

// ---------- Active sessions ----------
/** نشست‌های فعالِ کاربرِ جاری. */
export async function getSessions(): Promise<SessionInfo[]> {
  const res = await api.get<{ sessions: SessionInfo[] }>('/auth/sessions')
  return res.data.sessions
}

// ---------- Login history ----------
/** سه ورودِ اخیرِ کاربرِ جاری (موفق و ناموفق). */
export async function getLoginHistory(): Promise<LoginHistoryItem[]> {
  const res = await api.get<{ history: LoginHistoryItem[] }>('/auth/login-history')
  return res.data.history
}

/**
 * ثبتِ ورودِ یک تبِ تازه که با توکنِ موجود وارد شده (auto-resume): یک نشستِ فعالِ
 * جدید و یک ردیفِ تاریخچه می‌سازد و توکن‌های جدیدِ همین نشست را برمی‌گرداند.
 */
export async function registerVisit() {
  const res = await api.post('/auth/register-visit')
  return res.data as { accessToken: string; refreshToken: string; expireAt: string }
}

/** قطعِ یکی از نشست‌های کاربرِ جاری (خروج از راه دور). */
export async function revokeSession(sessionId: number) {
  const res = await api.post(`/auth/sessions/${sessionId}/revoke`)
  return res.data as { message: string }
}

/** فعال/غیرفعال کردن حساب کاربری کارمند (دسترسی ورود و امکان درخواست وام). */
export async function setAccountStatus(employeeId: string, isActive: boolean) {
  const res = await api.put(`/employee/${employeeId}/account-status`, { isActive })
  return res.data as { employeeId: string; isActive: boolean; message: string }
}

/** ویرایشِ کاملِ اطلاعاتِ یک کارمند/ادمین توسط ادمینِ ارشد. */
export interface UpdateEmployeePayload {
  firstName: string
  lastName: string
  username: string
  personnelNumber: string
  nationalId: string
  phoneNumber?: string | null
  email?: string | null
  hireDate: string
  marriageDate?: string | null
  role: string
  isActive: boolean
  jobPositionId?: number | null
  monthlySalary?: number | null
}

export async function updateEmployeeByAdmin(employeeId: string, payload: UpdateEmployeePayload) {
  const res = await api.put(`/employee/${employeeId}`, payload)
  return res.data
}

/** تعیین/ویرایشِ کد ملیِ یک کارمند (دقیقاً ۱۰ رقم). */
export async function setNationalId(employeeId: string, nationalId: string) {
  const res = await api.put(`/employee/${employeeId}/national-id`, { nationalId })
  return res.data as { employeeId: string; nationalId: string; message: string }
}

/** حذفِ نرمِ کارمند؛ سوابق و وام‌های او حفظ می‌شوند. */
export async function deleteEmployee(employeeId: string) {
  const res = await api.delete(`/employee/${employeeId}`)
  return res.data as { employeeId: string; message: string }
}

/** بازگرداندنِ کارمندِ حذف‌شده (به‌صورتِ غیرفعال). */
export async function restoreEmployee(employeeId: string) {
  const res = await api.post(`/employee/${employeeId}/restore`)
  return res.data as { employeeId: string; message: string }
}

/** تغییر سطح دسترسی یک ادمین: ارشد کردن، یا سپردنِ یک نوع وام به او. */
export async function setAdminScope(
  employeeId: string,
  payload: { isSeniorAdmin: boolean; managedLoanTypeId?: number | null },
) {
  const res = await api.put(`/employee/${employeeId}/admin-scope`, payload)
  return res.data as {
    employeeId: string
    isSeniorAdmin: boolean
    managedLoanTypeId?: number | null
    managedLoanTypeName?: string | null
    message: string
  }
}
