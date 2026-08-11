import { api } from './client'
import type {
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
  personnelNumber: string
  username: string
  hireDate: string
  role?: string
  /** برای نقش Employee الزامی است؛ حقوق و سقف وام از روی آن حساب می‌شود. */
  jobPositionId?: number
  /** حقوق اختصاصی؛ اگر خالی باشد حقوق پایه‌ی سمت استفاده می‌شود. */
  monthlySalary?: number
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
