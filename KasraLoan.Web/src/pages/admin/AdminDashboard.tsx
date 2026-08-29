import { useEffect, useState } from 'react'
import {
  Row,
  Col,
  Card,
  Table,
  Tag,
  Button,
  Switch,
  Form,
  Input,
  DatePicker,
  Select,
  Space,
  Modal,
  Alert,
  Popconfirm,
  Segmented,
  Progress,
  Drawer,
  App,
} from 'antd'
import {
  FileProtectOutlined,
  BankOutlined,
  UserAddOutlined,
  TeamOutlined,
  CrownOutlined,
  AuditOutlined,
  FileImageOutlined,
  ArrowRightOutlined,
  LockOutlined,
  DatabaseOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { DashboardLayout } from '../../components/DashboardLayout'
import { ProfilePanel } from '../../components/ProfilePanel'
import { isValidNationalId } from '../../utils/nationalId'
import { EditEmployeeModal } from './EditEmployeeModal'
import { useAuth } from '../../auth/AuthContext'
import {
  getAllPermissionRequests,
  approvePermissionRequest,
  rejectPermissionRequest,
  getLoanTypes,
  setLoanTypeStatus,
  createEmployee,
  getNextIdentifier,
  getAllEmployees,
  setAccountStatus,
  setNationalId,
  deleteEmployee,
  restoreEmployee,
  setAdminScope,
  getRequestPool,
  getAllLoans,
  approveLoan,
  rejectLoan,
  getLoanInstallments,
  getJobPositions,
  getUnreadCount,
  getPendingCheques,
  confirmCheque,
  rejectCheque,
  getLoanDocuments,
} from '../../api/services'
import type { JobPosition, RequestPoolItem } from '../../api/services'
import type {
  LoanType,
  LoanPermissionRequestItem,
  AdminLoanItem,
  LoanInstallment,
  InstallmentPaymentItem,
  LoanDocumentItem,
} from '../../api/types'

const statusTag: Record<string, { color: string; label: string }> = {
  Pending: { color: 'gold', label: 'در انتظار' },
  Approved: { color: 'green', label: 'تأیید شده' },
  Rejected: { color: 'red', label: 'رد شده' },
  Active: { color: 'blue', label: 'فعال' },
  Paid: { color: 'green', label: 'تسویه شده' },
  Closed: { color: 'default', label: 'بسته شده' },
}

export function AdminDashboard() {
  const { user } = useAuth()
  // ادمین ارشد به همه‌چیز دسترسی دارد؛ «ادمین وام» فقط به وامِ خودش.
  const isSenior = user?.isSeniorAdmin ?? false

  const [section, setSection] = useState('loanRequests')
  const [profileOpen, setProfileOpen] = useState(false)
  const [unread, setUnread] = useState(0)

  // تعداد نوتیف خوانده‌نشده برای نمایش روی عکسِ پروفایل، مثل داشبورد کارمند.
  useEffect(() => {
    getUnreadCount().then(setUnread).catch(() => {})
    const timer = setInterval(() => getUnreadCount().then(setUnread).catch(() => {}), 30000)
    return () => clearInterval(timer)
  }, [])

  // منوی مشترکِ کارِ وام (برای هر دو نوع ادمین). برای ادمین وام، برچسب «تنظیمات»
  // فقط وامِ خودش را نشان می‌دهد.
  const loanMenu = [
    { key: 'loanRequests', icon: <AuditOutlined />, label: 'درخواست‌های وام' },
    { key: 'cheques', icon: <FileImageOutlined />, label: 'چک‌های در انتظار' },
    { key: 'permissions', icon: <FileProtectOutlined />, label: 'درخواست‌های مجوز' },
    { key: 'loans', icon: <BankOutlined />, label: isSenior ? 'مدیریت وام‌ها' : 'تنظیمات وام' },
  ]

  // فقط ادمین ارشد کارهای مدیریتیِ کل سیستم را می‌بیند.
  const seniorOnlyMenu = [
    { key: 'addEmployee', icon: <UserAddOutlined />, label: 'افزودن کاربر' },
    { key: 'accesses', icon: <LockOutlined />, label: 'دسترسی‌ها' },
    {
      key: 'people',
      icon: <TeamOutlined />,
      label: 'لیست افراد',
      children: [
        { key: 'employees', icon: <TeamOutlined />, label: 'لیست کارمندان' },
        { key: 'admins', icon: <CrownOutlined />, label: 'لیست ادمین‌ها' },
      ],
    },
  ]

  // «همه‌ی درخواست‌ها» (استخرِ درخواست‌ها) فقط برای ادمین ارشد و در صدرِ منو.
  const requestPoolItem = {
    key: 'requestPool',
    icon: <DatabaseOutlined />,
    label: 'همه‌ی درخواست‌ها',
  }

  const menuItems = isSenior
    ? [requestPoolItem, ...loanMenu, ...seniorOnlyMenu]
    : loanMenu

  return (
    <DashboardLayout
      menuItems={menuItems}
      selectedKey={section}
      onSelect={setSection}
      hideLogout
      collapsedRail
      onAvatarClick={() => setProfileOpen(true)}
      avatarBadgeCount={unread}
    >
      {!isSenior && user?.managedLoanTypeName && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message={`ادمین وام «${user.managedLoanTypeName}»`}
          description="شما فقط به درخواست‌ها، مجوزها، چک‌ها و تنظیماتِ همین وام دسترسی دارید."
        />
      )}

      {isSenior && section === 'requestPool' && <RequestPoolSection />}
      {section === 'loanRequests' && <LoanRequestsSection />}
      {section === 'cheques' && <ChequeQueueSection />}
      {section === 'permissions' && <PermissionRequestsSection />}
      {section === 'loans' && <LoanManagementSection />}
      {isSenior && section === 'addEmployee' && <AddEmployeeSection />}
      {isSenior && section === 'accesses' && <AccessesSection />}
      {isSenior && section === 'employees' && <PeopleSection role="Employee" title="لیست کارمندان" />}
      {isSenior && section === 'admins' && <PeopleSection role="Admin" title="لیست ادمین‌ها" />}

      <Drawer
        placement="right"
        width={438}
        open={profileOpen}
        onClose={() => setProfileOpen(false)}
        closable={false}
        title={
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              width: '100%',
            }}
          >
            <span>پروفایل</span>
            <Button
              type="text"
              icon={<ArrowRightOutlined />}
              onClick={() => setProfileOpen(false)}
            />
          </div>
        }
        styles={{ body: { display: 'flex', flexDirection: 'column' } }}
      >
        <ProfilePanel />
      </Drawer>
    </DashboardLayout>
  )
}

/**
 * «همه‌ی درخواست‌ها» — استخرِ یکپارچه‌ی همه‌ی درخواست‌های کارمندان (وام + مجوز وام).
 * فقط‌خواندنی و مخصوص ادمین ارشد؛ نمای کلیِ همان داده‌ای که ادمین‌های وام
 * فیلترشده می‌بینند.
 */
function RequestPoolSection() {
  const [items, setItems] = useState<RequestPoolItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getRequestPool()
      .then(setItems)
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [])

  const columns: ColumnsType<RequestPoolItem> = [
    {
      title: 'نوع درخواست',
      dataIndex: 'categoryLabel',
      render: (v: string, r) => (
        <Tag color={r.category === 'Loan' ? 'blue' : 'purple'}>{v}</Tag>
      ),
    },
    { title: 'نوع وام', dataIndex: 'loanTypeName' },
    {
      title: 'درخواست‌دهنده',
      render: (_, r) => `${r.employeeName} (${r.employeeUsername})`,
    },
    { title: 'جزئیات', dataIndex: 'detail', ellipsis: true, render: (v?: string) => v || '—' },
    {
      title: 'وضعیت',
      dataIndex: 'status',
      render: (s: string) => <Tag color={statusTag[s]?.color}>{statusTag[s]?.label ?? s}</Tag>,
    },
    {
      title: 'تاریخ',
      dataIndex: 'createdAt',
      render: (d: string) => new Date(d).toLocaleDateString('fa-IR'),
    },
  ]

  return (
    <Card title={`همه‌ی درخواست‌ها (${items.length})`}>
      <Alert
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
        message="استخرِ درخواست‌ها"
        description="همه‌ی درخواست‌هایی که کارمندان ثبت کرده‌اند (وام و مجوز وام) یکجا. این نمای کلی مخصوص ادمین ارشد است؛ هر ادمین وام همین داده را فیلترشده برای وام خودش می‌بیند."
      />
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={items}
        pagination={{ pageSize: 15 }}
        scroll={{ x: 'max-content' }}
        locale={{ emptyText: 'هنوز درخواستی ثبت نشده' }}
      />
    </Card>
  )
}

/**
 * صف بررسی درخواست‌های وام.
 *
 * تأیید همین‌جا اقساط را می‌سازد، پس عمداً مبلغ و تعداد اقساط قبل از تأیید
 * در جدول دیده می‌شوند تا ادمین کورکورانه تأیید نکند.
 */
function LoanRequestsSection() {
  const { message } = App.useApp()
  const [items, setItems] = useState<AdminLoanItem[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<string | null>(null)
  const [detail, setDetail] = useState<{ loan: AdminLoanItem; installments: LoanInstallment[] } | null>(null)
  const [docs, setDocs] = useState<{ loan: AdminLoanItem; items: LoanDocumentItem[] } | null>(null)
  const [tab, setTab] = useState('pending')

  async function load() {
    setLoading(true)
    try {
      setItems(await getAllLoans())
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function approve(item: AdminLoanItem) {
    setBusyId(item.id)
    try {
      await approveLoan(item.id)
      message.success('وام تأیید شد و اقساط ساخته شدند.')
      await load()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در تأیید وام.')
    } finally {
      setBusyId(null)
    }
  }

  function reject(item: AdminLoanItem) {
    let reason = ''
    Modal.confirm({
      title: 'رد درخواست وام',
      content: (
        <Input.TextArea
          rows={3}
          placeholder="دلیل رد (به کارمند نمایش داده می‌شود)"
          onChange={(e) => (reason = e.target.value)}
        />
      ),
      okText: 'رد کن',
      okButtonProps: { danger: true },
      cancelText: 'انصراف',
      onOk: async () => {
        try {
          await rejectLoan(item.id, reason)
          message.success('درخواست وام رد شد.')
          await load()
        } catch (err: unknown) {
          const e = err as { response?: { data?: { message?: string } } }
          message.error(e.response?.data?.message ?? 'خطا در رد وام.')
        }
      },
    })
  }

  async function showInstallments(loan: AdminLoanItem) {
    const installments = await getLoanInstallments(loan.id)
    setDetail({ loan, installments: installments.sort((a, b) => a.installmentNumber - b.installmentNumber) })
  }

  async function showDocuments(loan: AdminLoanItem) {
    try {
      setDocs({ loan, items: await getLoanDocuments(loan.id) })
    } catch {
      message.error('خطا در دریافت مدارک.')
    }
  }

  const money = (v: number) => (v > 0 ? `${v.toLocaleString('fa-IR')} تومان` : '—')

  const columns: ColumnsType<AdminLoanItem> = [
    { title: 'کارمند', dataIndex: 'employeeName' },
    { title: 'نوع وام', dataIndex: 'loanTypeName' },
    { title: 'مبلغ درخواستی', dataIndex: 'requestedAmount', render: money },
    { title: 'مبلغ تأییدشده', dataIndex: 'approvedAmount', render: money },
    { title: 'اقساط', dataIndex: 'installmentCount' },
    { title: 'قسط ماهانه', dataIndex: 'monthlyPaymentAmount', render: money },
    {
      // بدون این ستون، ادمین وامی را که مدرک می‌خواهد کورکورانه تأیید می‌کرد.
      title: 'مدرک',
      render: (_, item) => {
        if (!item.requiresDocument) return <span style={{ color: '#999' }}>لازم نیست</span>

        return item.hasDocument ? (
          <Button size="small" onClick={() => showDocuments(item)}>
            مشاهده
          </Button>
        ) : (
          <Tag color="red">بارگذاری نشده</Tag>
        )
      },
    },
    {
      title: 'وضعیت',
      dataIndex: 'status',
      render: (s: string) => <Tag color={statusTag[s]?.color}>{statusTag[s]?.label ?? s}</Tag>,
    },
    {
      // پیشرفت اقساط فقط برای وام‌هایی که اقساط دارند معنی دارد.
      title: 'پیشرفت اقساط',
      render: (_, item) =>
        item.totalInstallments > 0 ? (
          <div style={{ minWidth: 120 }}>
            <Progress
              percent={Math.round((item.paidInstallments / item.totalInstallments) * 100)}
              size="small"
              status={item.paidInstallments >= item.totalInstallments ? 'success' : 'active'}
              format={() =>
                `${item.paidInstallments.toLocaleString('fa-IR')}/${item.totalInstallments.toLocaleString('fa-IR')}`
              }
            />
          </div>
        ) : (
          <span style={{ color: '#999' }}>—</span>
        ),
    },
    {
      title: 'تاریخ',
      dataIndex: 'createdAt',
      render: (d: string) => new Date(d).toLocaleDateString('fa-IR'),
    },
    {
      title: 'عملیات',
      render: (_, item) =>
        item.status === 'Pending' ? (
          <Space>
            <Popconfirm
              title="تأیید این وام؟"
              description="با تأیید، اقساط ساخته می‌شوند."
              onConfirm={() => approve(item)}
              okText="بله"
              cancelText="خیر"
              disabled={item.requiresDocument && !item.hasDocument}
            >
              <Button
                type="primary"
                size="small"
                loading={busyId === item.id}
                // سرور هم جلویش را می‌گیرد؛ اینجا فقط زودتر و با دلیل روشن.
                disabled={item.requiresDocument && !item.hasDocument}
                title={
                  item.requiresDocument && !item.hasDocument
                    ? 'تا مدرک بارگذاری نشود قابل تأیید نیست'
                    : undefined
                }
              >
                تأیید
              </Button>
            </Popconfirm>
            <Button danger size="small" onClick={() => reject(item)}>
              رد
            </Button>
          </Space>
        ) : item.status === 'Rejected' ? (
          <span style={{ color: '#999' }}>رد شده</span>
        ) : (
          <Button size="small" onClick={() => showInstallments(item)}>
            اقساط
          </Button>
        ),
    },
  ]

  // فاز هر وام از روی وضعیت + پیشرفت اقساط تعیین می‌شود:
  //  approved  = تأییدشده ولی هنوز قسطی پرداخت نشده
  //  active    = در حال بازپرداخت (بعضی اقساط پرداخت شده، نه همه)
  //  paid      = همه‌ی اقساط پرداخت شده
  function phaseOf(l: AdminLoanItem): string {
    if (l.status === 'Pending') return 'pending'
    if (l.status === 'Rejected') return 'rejected'
    if (l.status === 'Paid' || (l.totalInstallments > 0 && l.paidInstallments >= l.totalInstallments))
      return 'paid'
    if (l.paidInstallments > 0) return 'active'
    return 'approved'
  }

  const counts = {
    pending: items.filter((i) => phaseOf(i) === 'pending').length,
    approved: items.filter((i) => phaseOf(i) === 'approved').length,
    active: items.filter((i) => phaseOf(i) === 'active').length,
    paid: items.filter((i) => phaseOf(i) === 'paid').length,
    rejected: items.filter((i) => phaseOf(i) === 'rejected').length,
  }

  const visible = items.filter((i) => phaseOf(i) === tab)

  const tabLabel = (label: string, n: number) =>
    n > 0 ? `${label} (${n.toLocaleString('fa-IR')})` : label

  return (
    <>
      <Card
        title="مدیریت وام‌ها"
        extra={<Button onClick={load}>بروزرسانی</Button>}
      >
        <Segmented
          value={tab}
          onChange={(v) => setTab(v as string)}
          style={{ marginBottom: 16 }}
          options={[
            { value: 'pending', label: tabLabel('در انتظار بررسی', counts.pending) },
            { value: 'approved', label: tabLabel('تأییدشده', counts.approved) },
            { value: 'active', label: tabLabel('فعال', counts.active) },
            { value: 'paid', label: tabLabel('تسویه‌شده', counts.paid) },
            { value: 'rejected', label: tabLabel('رد شده', counts.rejected) },
          ]}
        />
        <Table
          rowKey="id"
          loading={loading}
          columns={columns}
          dataSource={visible}
          pagination={{ pageSize: 8 }}
          locale={{ emptyText: 'وامی در این بخش نیست' }}
          scroll={{ x: 'max-content' }}
        />
      </Card>

      <Modal
        open={!!docs}
        onCancel={() => setDocs(null)}
        footer={null}
        width={720}
        title={docs ? `مدارک ${docs.loan.loanTypeName} — ${docs.loan.employeeName}` : ''}
      >
        {docs && (
          <>
            <Alert
              type="info"
              style={{ marginBottom: 16 }}
              message={`مدرک لازم: ${docs.loan.requiredDocumentDescription ?? '—'}`}
            />
            {docs.items.length === 0 ? (
              <Alert type="warning" showIcon message="مدرکی بارگذاری نشده است." />
            ) : (
              docs.items.map((d) => (
                <div key={d.id} style={{ marginBottom: 16 }}>
                  <div style={{ marginBottom: 8, display: 'flex', justifyContent: 'space-between' }}>
                    <span style={{ fontWeight: 600 }}>{d.fileName}</span>
                    <span style={{ color: '#999', fontSize: 12 }}>
                      {new Date(d.uploadedAt).toLocaleDateString('fa-IR')}
                    </span>
                  </div>
                  {/* PDF در تگ img رندر نمی‌شود، پس لینک باز کردن داده می‌شود. */}
                  {d.filePath.toLowerCase().endsWith('.pdf') ? (
                    <Button href={d.filePath} target="_blank" rel="noreferrer">
                      باز کردن فایل PDF
                    </Button>
                  ) : (
                    <img
                      src={d.filePath}
                      alt={d.fileName}
                      style={{ width: '100%', borderRadius: 8 }}
                    />
                  )}
                </div>
              ))
            )}
          </>
        )}
      </Modal>

      <Modal
        open={!!detail}
        onCancel={() => setDetail(null)}
        footer={null}
        width={720}
        title={detail ? `اقساط ${detail.loan.loanTypeName} — ${detail.loan.employeeName}` : ''}
      >
        {detail && (
          <>
            <Alert
              type="info"
              style={{ marginBottom: 16 }}
              message={`کل بازپرداخت: ${money(detail.loan.totalPayableAmount)} (اصل ${money(detail.loan.approvedAmount)} + کارمزد ${detail.loan.annualFeePercent}٪ سالانه)`}
            />
            <Table
              rowKey="id"
              size="small"
              dataSource={detail.installments}
              pagination={{ pageSize: 12 }}
              scroll={{ x: 'max-content' }}
              columns={[
                { title: 'شماره', dataIndex: 'installmentNumber', width: 80 },
                { title: 'مبلغ', dataIndex: 'amount', render: money },
                {
                  title: 'سررسید',
                  dataIndex: 'dueDate',
                  render: (d: string) => new Date(d).toLocaleDateString('fa-IR'),
                },
                {
                  title: 'وضعیت',
                  dataIndex: 'isPaid',
                  render: (p: boolean) =>
                    p ? <Tag color="green">پرداخت شده</Tag> : <Tag color="gold">پرداخت نشده</Tag>,
                },
              ]}
            />
          </>
        )}
      </Modal>
    </>
  )
}

/**
 * صف چک‌های منتظر تأیید.
 *
 * قدیمی‌ترین اول می‌آید چون نزدیک‌ترین به قطعی شدن لیست حقوق است — چکی که تا
 * آن موقع تعیین تکلیف نشود، قسطش از حقوق کسر می‌شود.
 */
function ChequeQueueSection() {
  const { message } = App.useApp()
  const [items, setItems] = useState<InstallmentPaymentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<string | null>(null)
  const [preview, setPreview] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    try {
      setItems(await getPendingCheques())
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function confirm(item: InstallmentPaymentItem) {
    setBusyId(item.id)
    try {
      await confirmCheque(item.id)
      message.success('چک تأیید شد و قسط تسویه گردید.')
      await load()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در تأیید چک.')
    } finally {
      setBusyId(null)
    }
  }

  function reject(item: InstallmentPaymentItem) {
    let reason = ''
    Modal.confirm({
      title: 'رد چک',
      content: (
        <Input.TextArea
          rows={3}
          placeholder="دلیل رد (الزامی — به کارمند نمایش داده می‌شود)"
          onChange={(e) => (reason = e.target.value)}
        />
      ),
      okText: 'رد کن',
      okButtonProps: { danger: true },
      cancelText: 'انصراف',
      onOk: async () => {
        try {
          await rejectCheque(item.id, reason)
          message.success('چک رد شد.')
          await load()
        } catch (err: unknown) {
          const e = err as { response?: { data?: { message?: string } } }
          message.error(e.response?.data?.message ?? 'خطا در رد چک.')
        }
      },
    })
  }

  const money = (v: number) => `${v.toLocaleString('fa-IR')} تومان`

  const columns: ColumnsType<InstallmentPaymentItem> = [
    { title: 'کارمند', dataIndex: 'employeeName', render: (v?: string) => v || '—' },
    { title: 'نوع وام', dataIndex: 'loanTypeName', render: (v?: string) => v || '—' },
    { title: 'قسط', dataIndex: 'installmentNumber' },
    { title: 'مبلغ', dataIndex: 'amount', render: money },
    { title: 'شماره چک', dataIndex: 'chequeNumber' },
    { title: 'بانک', dataIndex: 'chequeBankName', render: (v?: string) => v || '—' },
    { title: 'تاریخ چک', dataIndex: 'chequeDatePersian', render: (v?: string) => v || '—' },
    {
      title: 'تصویر',
      render: (_, item) =>
        item.chequeImageUrl ? (
          <Button size="small" onClick={() => setPreview(item.chequeImageUrl!)}>
            مشاهده
          </Button>
        ) : (
          '—'
        ),
    },
    {
      title: 'عملیات',
      render: (_, item) => (
        <Space>
          <Popconfirm
            title="تأیید این چک؟"
            description="با تأیید، قسط تسویه‌شده ثبت می‌شود."
            onConfirm={() => confirm(item)}
            okText="بله"
            cancelText="خیر"
          >
            <Button type="primary" size="small" loading={busyId === item.id}>
              تأیید
            </Button>
          </Popconfirm>
          <Button danger size="small" onClick={() => reject(item)}>
            رد
          </Button>
        </Space>
      ),
    },
  ]

  return (
    <>
      <Card
        title={`چک‌های در انتظار تأیید${items.length > 0 ? ` — ${items.length} مورد` : ''}`}
        extra={<Button onClick={load}>بروزرسانی</Button>}
      >
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="چک تا تأیید شما پرداخت‌شده محسوب نمی‌شود"
          description="هر چکی که تا قطعی شدن لیست حقوق تعیین تکلیف نشود، قسطش از حقوق کارمند کسر خواهد شد. قدیمی‌ترین چک‌ها بالای فهرست‌اند."
        />
        <Table
          rowKey="id"
          loading={loading}
          columns={columns}
          dataSource={items}
          pagination={{ pageSize: 8 }}
          locale={{ emptyText: 'چکی در انتظار بررسی نیست' }}
          scroll={{ x: 'max-content' }}
        />
      </Card>

      <Modal
        open={!!preview}
        onCancel={() => setPreview(null)}
        footer={null}
        title="تصویر چک"
      >
        {preview && (
          <img src={preview} alt="cheque" style={{ width: '100%', borderRadius: 8 }} />
        )}
      </Modal>
    </>
  )
}

function PermissionRequestsSection() {
  const { message } = App.useApp()
  const [items, setItems] = useState<LoanPermissionRequestItem[]>([])
  const [loading, setLoading] = useState(true)

  async function load() {
    setLoading(true)
    try {
      setItems(await getAllPermissionRequests())
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function approve(id: string) {
    await approvePermissionRequest(id)
    message.success('درخواست تأیید شد و مجوز فعال گردید.')
    load()
  }

  function reject(item: LoanPermissionRequestItem) {
    let reason = ''
    Modal.confirm({
      title: 'رد درخواست مجوز',
      content: (
        <Input.TextArea
          rows={3}
          placeholder="دلیل رد (اختیاری)"
          onChange={(e) => (reason = e.target.value)}
        />
      ),
      okText: 'رد کن',
      okButtonProps: { danger: true },
      cancelText: 'انصراف',
      onOk: async () => {
        await rejectPermissionRequest(item.id, reason)
        message.success('درخواست رد شد.')
        load()
      },
    })
  }

  const columns: ColumnsType<LoanPermissionRequestItem> = [
    { title: 'کارمند', dataIndex: 'employeeName' },
    { title: 'نوع وام', dataIndex: 'loanTypeName' },
    { title: 'دلیل', dataIndex: 'reason', ellipsis: true },
    {
      title: 'وضعیت',
      dataIndex: 'status',
      render: (s: string) => <Tag color={statusTag[s]?.color}>{statusTag[s]?.label ?? s}</Tag>,
    },
    {
      title: 'تاریخ',
      dataIndex: 'createdAt',
      render: (d: string) => new Date(d).toLocaleDateString('fa-IR'),
    },
    {
      title: 'عملیات',
      render: (_, item) =>
        item.status === 'Pending' ? (
          <Space>
            <Popconfirm title="تأیید این درخواست؟" onConfirm={() => approve(item.id)} okText="بله" cancelText="خیر">
              <Button type="primary" size="small">
                تأیید
              </Button>
            </Popconfirm>
            <Button danger size="small" onClick={() => reject(item)}>
              رد
            </Button>
          </Space>
        ) : (
          <span style={{ color: '#999' }}>بررسی شده</span>
        ),
    },
  ]

  return (
    <Card title="درخواست‌های مجوز وام">
      <Table rowKey="id" loading={loading} columns={columns} dataSource={items} pagination={{ pageSize: 8 }} scroll={{ x: 'max-content' }} />
    </Card>
  )
}

function LoanManagementSection() {
  const { message } = App.useApp()
  const { user } = useAuth()
  const isSenior = user?.isSeniorAdmin ?? false
  const [loans, setLoans] = useState<LoanType[]>([])
  const [loading, setLoading] = useState(true)

  async function load() {
    setLoading(true)
    try {
      const all = await getLoanTypes()
      // ادمین وام فقط تنظیماتِ وامِ خودش را می‌بیند و می‌تواند عوض کند.
      setLoans(isSenior ? all : all.filter((l) => l.id === user?.managedLoanTypeId))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isSenior, user?.managedLoanTypeId])

  async function toggle(loan: LoanType, isActive: boolean) {
    await setLoanTypeStatus(loan.id, isActive)
    message.success(isActive ? `«${loan.name}» فعال شد.` : `«${loan.name}» غیرفعال شد.`)
    setLoans((prev) => prev.map((l) => (l.id === loan.id ? { ...l, isActive } : l)))
  }

  const columns: ColumnsType<LoanType> = [
    { title: 'نام وام', dataIndex: 'name' },
    { title: 'نوع', dataIndex: 'type' },
    {
      title: 'وضعیت',
      dataIndex: 'isActive',
      render: (v: boolean) => (v ? <Tag color="green">فعال</Tag> : <Tag>غیرفعال</Tag>),
    },
    {
      title: 'تغییر وضعیت',
      render: (_, loan) => (
        <Switch checked={loan.isActive} onChange={(checked) => toggle(loan, checked)} />
      ),
    },
  ]

  return (
    <Card title="مدیریت انواع وام">
      <Table rowKey="id" loading={loading} columns={columns} dataSource={loans} pagination={false} scroll={{ x: 'max-content' }} />
    </Card>
  )
}

function AddEmployeeSection() {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)
  const [created, setCreated] = useState<{ username: string } | null>(null)
  const [positions, setPositions] = useState<JobPosition[]>([])
  const [loanTypes, setLoanTypes] = useState<LoanType[]>([])
  const [autoId, setAutoId] = useState<string | null>(null)
  const [autoIdLoading, setAutoIdLoading] = useState(false)
  const role = Form.useWatch('role', form)
  const adminType = Form.useWatch('adminType', form)
  const jobPositionId = Form.useWatch('jobPositionId', form)
  const hireDate = Form.useWatch<{ toISOString: () => string } | undefined>('hireDate', form)

  useEffect(() => {
    getJobPositions(true).then(setPositions).catch(() => {})
    getLoanTypes().then(setLoanTypes).catch(() => {})
  }, [])

  // پیش‌نمایش کد ۹ رقمی: وقتی سمت و تاریخ استخدام هر دو پر شدند (و نقش کارمند است)
  // از سرور می‌پرسیم عدد بعدی چه می‌شود و همان‌جا فقط‌خواندنی نشانش می‌دهیم.
  useEffect(() => {
    if (role === 'Admin' || !jobPositionId || !hireDate) {
      setAutoId(null)
      return
    }
    let cancelled = false
    setAutoIdLoading(true)
    getNextIdentifier(jobPositionId, hireDate.toISOString())
      .then((id) => { if (!cancelled) setAutoId(id) })
      .catch(() => { if (!cancelled) setAutoId(null) })
      .finally(() => { if (!cancelled) setAutoIdLoading(false) })
    return () => { cancelled = true }
  }, [role, jobPositionId, hireDate])

  async function onFinish(values: {
    firstName: string
    lastName: string
    nationalId: string
    password: string
    personnelNumber?: string
    username?: string
    hireDate: { toISOString: () => string }
    role: string
    jobPositionId?: number
    adminType?: string
    managedLoanTypeId?: number
  }) {
    const isAdmin = values.role === 'Admin'
    const isSeniorAdmin = isAdmin && values.adminType !== 'loan'
    setSubmitting(true)
    try {
      const res = await createEmployee({
        firstName: values.firstName,
        lastName: values.lastName,
        nationalId: values.nationalId,
        password: values.password,
        // شماره پرسنلی و نام کاربری فقط برای ادمین دستی‌اند؛ برای کارمند سرور خودش
        // یک عددِ ۹ رقمیِ یکسان برای هر دو می‌سازد.
        personnelNumber: isAdmin ? values.personnelNumber : undefined,
        username: isAdmin ? values.username : undefined,
        hireDate: values.hireDate.toISOString(),
        role: values.role,
        jobPositionId: values.jobPositionId,
        isSeniorAdmin: isAdmin ? isSeniorAdmin : undefined,
        managedLoanTypeId: isAdmin && !isSeniorAdmin ? values.managedLoanTypeId : undefined,
      })
      setCreated({ username: res.username })
      message.success('کاربر ایجاد شد.')
      form.resetFields()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ایجاد کاربر.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} lg={12}>
        <Card title="افزودن کاربر جدید">
          <Alert
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
            message="نام کاربری کارمند خودکار است؛ رمز را خودتان تعیین کنید"
            description="برای کارمند، نام کاربری خودکار از روی سال استخدام و سمت شغلی ساخته می‌شود (مثلاً ۱۴۰۴۰۱۰۰۱). رمز عبور را همین‌جا خودتان تعیین می‌کنید و به کاربر می‌دهید؛ او می‌تواند بعداً از پروفایلش عوضش کند."
          />
          <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ role: 'Employee' }}>
            <Row gutter={12}>
              <Col span={12}>
                <Form.Item label="نام" name="firstName" rules={[{ required: true }]}>
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="نام خانوادگی" name="lastName" rules={[{ required: true }]}>
                  <Input />
                </Form.Item>
              </Col>
            </Row>
            <Form.Item
              label="کد ملی"
              name="nationalId"
              rules={[
                { required: true, message: 'کد ملی را وارد کنید' },
                {
                  validator: (_, value) =>
                    !value || isValidNationalId(value)
                      ? Promise.resolve()
                      : Promise.reject(new Error('کد ملی معتبر نیست (۱۰ رقم با رقمِ کنترلیِ درست).')),
                },
              ]}
            >
              <Input maxLength={10} inputMode="numeric" placeholder="۱۰ رقم" style={{ direction: 'ltr', textAlign: 'right' }} />
            </Form.Item>
            <Form.Item
              label="رمز عبور"
              name="password"
              rules={[
                { required: true, message: 'رمز عبور را وارد کنید' },
                { min: 6, message: 'رمز عبور باید حداقل ۶ کاراکتر باشد' },
              ]}
              extra="این رمز را به کاربر بدهید؛ او می‌تواند بعداً از پروفایلش عوضش کند."
            >
              <Input.Password autoComplete="new-password" />
            </Form.Item>
            {/* شماره پرسنلی و نام کاربری فقط برای ادمین دستی‌اند؛ کارمند هر دو را
                خودکار و یکسان می‌گیرد (پیش‌نمایش زیر سمت شغلی نشان داده می‌شود). */}
            {role === 'Admin' && (
              <>
                <Form.Item
                  label="شماره پرسنلی"
                  name="personnelNumber"
                  rules={[
                    { required: true, message: 'شماره پرسنلی الزامی است' },
                    { pattern: /^\d+$/, message: 'شماره پرسنلی فقط باید عدد باشد' },
                  ]}
                >
                  <Input />
                </Form.Item>
                <Form.Item label="نام کاربری" name="username" rules={[{ required: true }]}>
                  <Input />
                </Form.Item>
                <Form.Item
                  label="نوع ادمین"
                  name="adminType"
                  initialValue="senior"
                  rules={[{ required: true }]}
                >
                  <Select
                    options={[
                      { value: 'senior', label: 'ادمین ارشد (دسترسی کامل)' },
                      { value: 'loan', label: 'ادمین وام (فقط یک نوع وام)' },
                    ]}
                  />
                </Form.Item>
                {adminType === 'loan' && (
                  <Form.Item
                    label="وامِ تحت مدیریت"
                    name="managedLoanTypeId"
                    rules={[{ required: true, message: 'نوع وام را انتخاب کنید' }]}
                    extra="این ادمین فقط به درخواست‌ها، مجوزها، چک‌ها و تنظیماتِ همین وام دسترسی خواهد داشت."
                  >
                    <Select
                      placeholder="انتخاب کنید"
                      options={loanTypes.map((l) => ({ value: l.id, label: l.name }))}
                    />
                  </Form.Item>
                )}
              </>
            )}
            <Form.Item label="تاریخ استخدام" name="hireDate" rules={[{ required: true }]}>
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item label="نقش" name="role">
              <Select
                options={[
                  { value: 'Employee', label: 'کارمند' },
                  { value: 'Admin', label: 'ادمین' },
                ]}
              />
            </Form.Item>

            {/* سمت شغلی فقط برای کارمند لازم است: حقوق و سقف وام از روی آن حساب می‌شود. */}
            {role !== 'Admin' && (
              <>
                <Form.Item
                  label="سمت شغلی"
                  name="jobPositionId"
                  rules={[{ required: true, message: 'انتخاب سمت شغلی برای کارمند الزامی است' }]}
                  extra="حقوق و در نتیجه سقف وام کارمند از روی این سمت محاسبه می‌شود."
                >
                  <Select
                    placeholder="انتخاب کنید"
                    options={positions.map((p) => ({
                      value: p.id,
                      label: `${p.title} — ${p.baseSalary.toLocaleString('fa-IR')} تومان`,
                    }))}
                  />
                </Form.Item>

                {/* شماره پرسنلی خودکار: با انتخاب تاریخ استخدام و سمت، خودش پر می‌شود.
                    کاملاً قفل است — نه می‌شود تایپ کرد، نه حتی فوکوس گرفت. */}
                <Form.Item label="شماره پرسنلی">
                  <Input
                    readOnly
                    tabIndex={-1}
                    value={autoIdLoading ? 'در حال محاسبه…' : autoId ?? ''}
                    // placeholder="با انتخاب تاریخ استخدام و سمت شغلی، خودکار پر می‌شود"
                    prefix={<LockOutlined style={{ color: '#999', cursor: 'not-allowed' }} />}
                    style={{
                      direction: 'ltr',
                      background: 'rgba(0, 0, 0, 0.04)',
                      cursor: 'not-allowed',
                    }}
                    // نشانگرِ «ممنوع» باید روی خودِ input داخلی هم باشد، نه فقط قابِ بیرونی؛
                    // وگرنه وسط کادر نشانگر تایپ نشان داده می‌شود.
                    styles={{
                      input: {
                        textAlign: 'center',
                        fontFamily: 'monospace',
                        letterSpacing: 3,
                        fontWeight: 600,
                        cursor: 'not-allowed',
                      },
                      prefix: { cursor: 'not-allowed' },
                    }}
                  />
                </Form.Item>
              </>
            )}

            <Button type="primary" htmlType="submit" block loading={submitting}>
              ایجاد کاربر
            </Button>
          </Form>
        </Card>
      </Col>
      <Col xs={24} lg={12}>
        {created && (
          <Alert
            type="success"
            showIcon
            message="کاربر ساخته شد"
            description={
              <div>
                <div>نام کاربری: <b>{created.username}</b></div>
                <div style={{ marginTop: 8 }}>
                  کاربر با همین نام کاربری و رمزی که تعیین کردید می‌تواند وارد شود.
                </div>
              </div>
            }
          />
        )}
      </Col>
    </Row>
  )
}

interface EmployeeRow {
  id: string
  firstName: string
  lastName: string
  username: string
  personnelNumber: string
  nationalId?: string | null
  phoneNumber?: string | null
  email?: string | null
  hireDate?: string | null
  marriageDate?: string | null
  jobPositionId?: number | null
  monthlySalary?: number | null
  role: string
  isSeniorAdmin: boolean
  managedLoanTypeId?: number | null
  managedLoanTypeName?: string | null
  isActive: boolean
  isDeleted: boolean
  deletedAt?: string | null
  jobPositionTitle?: string | null
  effectiveMonthlySalary: number
  maxMonthlyInstallment: number
  employmentStatus: string
}

function PeopleSection({ role, title }: { role: 'Admin' | 'Employee'; title: string }) {
  const { message } = App.useApp()
  const [rows, setRows] = useState<EmployeeRow[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<string | null>(null)
  // نمای فعلی: همه / فعال / غیرفعال. «غیرفعال» شاملِ حذف‌شده‌ها هم هست.
  const [view, setView] = useState<'all' | 'active' | 'inactive'>('all')
  // ویرایشِ کد ملی (برای جایگزینیِ مقادیرِ موقتِ کاربرانِ قدیمی).
  const [editNid, setEditNid] = useState<EmployeeRow | null>(null)
  const [nidValue, setNidValue] = useState('')
  const [nidSaving, setNidSaving] = useState(false)
  // ویرایشِ کاملِ اطلاعاتِ کاربر (فقط ادمینِ ارشد — این بخش خودش پشتِ isSenior است).
  const [editRow, setEditRow] = useState<EmployeeRow | null>(null)

  function applyEdit(id: string, patch: Partial<EmployeeRow>) {
    setRows((prev) => prev.map((x) => (x.id === id ? { ...x, ...patch } : x)))
  }

  useEffect(() => {
    setLoading(true)
    getAllEmployees()
      .then((data) => setRows(Array.isArray(data) ? data : data.items ?? []))
      .finally(() => setLoading(false))
  }, [])

  function openEditNid(row: EmployeeRow) {
    setEditNid(row)
    setNidValue(row.nationalId ?? '')
  }

  async function saveNid() {
    if (!editNid) return
    if (!isValidNationalId(nidValue)) {
      message.error('کد ملی معتبر نیست (۱۰ رقم با رقمِ کنترلیِ درست).')
      return
    }
    setNidSaving(true)
    try {
      await setNationalId(editNid.id, nidValue)
      setRows((prev) => prev.map((x) => (x.id === editNid.id ? { ...x, nationalId: nidValue } : x)))
      message.success('کد ملی به‌روزرسانی شد.')
      setEditNid(null)
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در به‌روزرسانی کد ملی.')
    } finally {
      setNidSaving(false)
    }
  }

  // فقط افراد با نقش موردنظر همین بخش نمایش داده می‌شوند، بعد بر اساس نما فیلتر می‌شوند.
  const byRole = rows.filter((r) => r.role === role)
  const filtered = byRole.filter((r) =>
    view === 'active'
      ? r.isActive && !r.isDeleted
      : view === 'inactive'
        ? !r.isActive || r.isDeleted
        : true,
  )
  const activeCount = byRole.filter((r) => r.isActive && !r.isDeleted).length
  const inactiveCount = byRole.length - activeCount

  const money = (v: number) => (v > 0 ? `${v.toLocaleString('fa-IR')} تومان` : '—')

  async function toggleAccount(row: EmployeeRow, isActive: boolean) {
    setBusyId(row.id)
    try {
      await setAccountStatus(row.id, isActive)
      setRows((prev) => prev.map((x) => (x.id === row.id ? { ...x, isActive } : x)))
      message.success(
        isActive
          ? 'حساب کاربری فعال شد.'
          : 'حساب کاربری غیرفعال شد؛ کاربر دیگر نمی‌تواند وارد شود یا وام بگیرد.',
      )
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در تغییر وضعیت حساب.')
    } finally {
      setBusyId(null)
    }
  }

  async function removeRow(row: EmployeeRow) {
    setBusyId(row.id)
    try {
      await deleteEmployee(row.id)
      // حذفِ نرم: از فهرست حذف نمی‌شود، فقط علامت می‌خورد و غیرفعال می‌شود.
      setRows((prev) =>
        prev.map((x) => (x.id === row.id ? { ...x, isDeleted: true, isActive: false } : x)),
      )
      message.success('کارمند حذف شد؛ سوابق و وام‌های او حفظ شد.')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در حذف کارمند.')
    } finally {
      setBusyId(null)
    }
  }

  async function restoreRow(row: EmployeeRow) {
    setBusyId(row.id)
    try {
      await restoreEmployee(row.id)
      setRows((prev) => prev.map((x) => (x.id === row.id ? { ...x, isDeleted: false } : x)))
      message.success('کارمند بازگردانده شد (غیرفعال).')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در بازگردانی کارمند.')
    } finally {
      setBusyId(null)
    }
  }

  const columns: ColumnsType<EmployeeRow> = [
    { title: 'نام', render: (_, r) => `${r.firstName} ${r.lastName}` },
    { title: 'نام کاربری', dataIndex: 'username' },
    { title: 'شماره پرسنلی', dataIndex: 'personnelNumber' },
    {
      title: 'کد ملی',
      dataIndex: 'nationalId',
      render: (v: string | null | undefined, r: EmployeeRow) => (
        <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6, direction: 'ltr' }}>
          {v || '—'}
          <Button type="link" size="small" onClick={() => openEditNid(r)}>
            ویرایش
          </Button>
        </span>
      ),
    },
    ...(role === 'Employee'
      ? ([
          { title: 'سمت', dataIndex: 'jobPositionTitle', render: (v?: string) => v || '—' },
          { title: 'حقوق', dataIndex: 'effectiveMonthlySalary', render: money },
          { title: 'سقف قسط', dataIndex: 'maxMonthlyInstallment', render: money },
          {
            title: 'اشتغال',
            dataIndex: 'employmentStatus',
            render: (s: string) =>
              s === 'Terminated' ? <Tag color="red">پایان همکاری</Tag> : <Tag color="green">مشغول</Tag>,
          },
        ] as ColumnsType<EmployeeRow>)
      : []),
    ...(role === 'Admin'
      ? ([
          {
            title: 'سطح دسترسی',
            render: (_, r) =>
              r.isSeniorAdmin ? (
                <Tag color="gold">ادمین ارشد</Tag>
              ) : r.managedLoanTypeName ? (
                <Tag color="blue">وام: {r.managedLoanTypeName}</Tag>
              ) : (
                <Tag>تعیین‌نشده</Tag>
              ),
          },
        ] as ColumnsType<EmployeeRow>)
      : []),
    {
      title: 'حساب کاربری',
      dataIndex: 'isActive',
      // حذف‌شده: فقط تگ؛ کارمندِ عادی: سوییچ فعال/غیرفعال؛ ادمین: فقط نمایش.
      render: (v: boolean, r) =>
        r.isDeleted ? (
          <Tag color="red">حذف‌شده</Tag>
        ) : role === 'Employee' ? (
          <Switch
            checked={v}
            loading={busyId === r.id}
            checkedChildren="فعال"
            unCheckedChildren="غیرفعال"
            onChange={(checked) => toggleAccount(r, checked)}
          />
        ) : v ? (
          <Tag color="green">فعال</Tag>
        ) : (
          <Tag>غیرفعال</Tag>
        ),
    },
    {
      title: 'عملیات',
      render: (_: unknown, r: EmployeeRow) => (
        <Space>
          <Button size="small" onClick={() => setEditRow(r)}>
            ویرایش
          </Button>
          {role === 'Employee' &&
            (r.isDeleted ? (
              <Button size="small" loading={busyId === r.id} onClick={() => restoreRow(r)}>
                بازگردانی
              </Button>
            ) : (
              <Popconfirm
                title="حذف کارمند"
                description="کارمند حذف می‌شود ولی سوابق و وام‌هایش حفظ می‌ماند. مطمئنید؟"
                okText="حذف"
                okButtonProps={{ danger: true }}
                cancelText="انصراف"
                onConfirm={() => removeRow(r)}
              >
                <Button danger size="small" loading={busyId === r.id}>
                  حذف
                </Button>
              </Popconfirm>
            ))}
        </Space>
      ),
    },
  ]

  return (
    <>
      <Card title={`${title} (${filtered.length})`}>
        <div style={{ marginBottom: 12 }}>
          <Segmented
            value={view}
            onChange={(v) => setView(v as 'all' | 'active' | 'inactive')}
            options={[
              { label: `همه (${byRole.length})`, value: 'all' },
              { label: `فعال (${activeCount})`, value: 'active' },
              { label: `غیرفعال (${inactiveCount})`, value: 'inactive' },
            ]}
          />
        </div>
        <Table
          rowKey="id"
          loading={loading}
          columns={columns}
          dataSource={filtered}
          pagination={{ pageSize: 10 }}
          scroll={{ x: 'max-content' }}
        />
      </Card>

      <Modal
        open={!!editNid}
        onCancel={() => setEditNid(null)}
        onOk={saveNid}
        confirmLoading={nidSaving}
        title="ویرایش کد ملی"
        okText="ذخیره"
        cancelText="انصراف"
        centered
        destroyOnHidden
      >
        <div style={{ marginBottom: 8, color: 'var(--text-muted)' }}>
          {editNid ? `${editNid.firstName} ${editNid.lastName}` : ''}
        </div>
        <Input
          value={nidValue}
          onChange={(e) => setNidValue(e.target.value.replace(/\D/g, '').slice(0, 10))}
          maxLength={10}
          inputMode="numeric"
          placeholder="۱۰ رقم"
          style={{ direction: 'ltr', textAlign: 'right' }}
        />
      </Modal>

      <EditEmployeeModal employee={editRow} onClose={() => setEditRow(null)} onSaved={applyEdit} />
    </>
  )
}

/**
 * «دسترسی‌ها» — فقط ادمین ارشد. مشخص می‌کند هر ادمین ارشد است یا مسئول کدام وام.
 * هر ردیف یک انتخاب‌گر دارد: «ادمین ارشد» یا یکی از انواع وام.
 */
function AccessesSection() {
  const { message } = App.useApp()
  const { user } = useAuth()
  const [admins, setAdmins] = useState<EmployeeRow[]>([])
  const [loanTypes, setLoanTypes] = useState<LoanType[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    try {
      const data = await getAllEmployees()
      const rows: EmployeeRow[] = Array.isArray(data) ? data : data.items ?? []
      setAdmins(rows.filter((r) => r.role === 'Admin'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    getLoanTypes().then(setLoanTypes).catch(() => {})
  }, [])

  async function assign(row: EmployeeRow, value: string) {
    setBusyId(row.id)
    try {
      const payload =
        value === 'senior'
          ? { isSeniorAdmin: true }
          : { isSeniorAdmin: false, managedLoanTypeId: Number(value) }
      const res = await setAdminScope(row.id, payload)
      message.success(res.message)
      await load()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در تغییر دسترسی.')
    } finally {
      setBusyId(null)
    }
  }

  const columns: ColumnsType<EmployeeRow> = [
    { title: 'نام', render: (_, r) => `${r.firstName} ${r.lastName}` },
    { title: 'نام کاربری', dataIndex: 'username' },
    {
      title: 'سطح فعلی',
      render: (_, r) =>
        r.isSeniorAdmin ? (
          <Tag color="gold">ادمین ارشد</Tag>
        ) : r.managedLoanTypeName ? (
          <Tag color="blue">وام: {r.managedLoanTypeName}</Tag>
        ) : (
          <Tag>تعیین‌نشده</Tag>
        ),
    },
    {
      title: 'تغییر دسترسی',
      render: (_, r) => {
        const isSelf = r.id === user?.id
        return (
          <Select
            style={{ minWidth: 200 }}
            disabled={isSelf || busyId === r.id}
            loading={busyId === r.id}
            value={r.isSeniorAdmin ? 'senior' : r.managedLoanTypeId != null ? String(r.managedLoanTypeId) : undefined}
            placeholder="انتخاب کنید"
            onChange={(v) => assign(r, v)}
            options={[
              { value: 'senior', label: 'ادمین ارشد (دسترسی کامل)' },
              ...loanTypes.map((l) => ({ value: String(l.id), label: `ادمین وام: ${l.name}` })),
            ]}
          />
        )
      },
    },
  ]

  return (
    <Card title="دسترسی‌ها">
      <Alert
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
        message="سطح دسترسی هر ادمین را اینجا تعیین کنید"
        description="ادمین ارشد به همه‌چیز دسترسی دارد. «ادمین وام» فقط درخواست‌ها، مجوزها، چک‌ها و تنظیماتِ همان وام را می‌بیند. بعد از تغییر، ادمین باید یک‌بار دیگر وارد شود تا دسترسیِ جدید اعمال شود."
      />
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={admins}
        pagination={false}
        scroll={{ x: 'max-content' }}
      />
    </Card>
  )
}
