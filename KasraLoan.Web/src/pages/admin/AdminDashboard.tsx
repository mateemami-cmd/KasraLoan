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
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { DashboardLayout } from '../../components/DashboardLayout'
import { ProfilePanel } from '../../components/ProfilePanel'
import {
  getAllPermissionRequests,
  approvePermissionRequest,
  rejectPermissionRequest,
  getLoanTypes,
  setLoanTypeStatus,
  createEmployee,
  getAllEmployees,
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
import type { JobPosition } from '../../api/services'
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
  const [section, setSection] = useState('loanRequests')
  const [profileOpen, setProfileOpen] = useState(false)
  const [unread, setUnread] = useState(0)

  // تعداد نوتیف خوانده‌نشده برای نمایش روی عکسِ پروفایل، مثل داشبورد کارمند.
  useEffect(() => {
    getUnreadCount().then(setUnread).catch(() => {})
    const timer = setInterval(() => getUnreadCount().then(setUnread).catch(() => {}), 30000)
    return () => clearInterval(timer)
  }, [])

  const menuItems = [
    { key: 'loanRequests', icon: <AuditOutlined />, label: 'درخواست‌های وام' },
    { key: 'cheques', icon: <FileImageOutlined />, label: 'چک‌های در انتظار' },
    { key: 'permissions', icon: <FileProtectOutlined />, label: 'درخواست‌های مجوز' },
    { key: 'loans', icon: <BankOutlined />, label: 'مدیریت وام‌ها' },
    { key: 'addEmployee', icon: <UserAddOutlined />, label: 'افزودن کاربر' },
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
      {section === 'loanRequests' && <LoanRequestsSection />}
      {section === 'cheques' && <ChequeQueueSection />}
      {section === 'permissions' && <PermissionRequestsSection />}
      {section === 'loans' && <LoanManagementSection />}
      {section === 'addEmployee' && <AddEmployeeSection />}
      {section === 'employees' && <PeopleSection role="Employee" title="لیست کارمندان" />}
      {section === 'admins' && <PeopleSection role="Admin" title="لیست ادمین‌ها" />}

      <Drawer
        placement="right"
        width={400}
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
      <Table rowKey="id" loading={loading} columns={columns} dataSource={items} pagination={{ pageSize: 8 }} />
    </Card>
  )
}

function LoanManagementSection() {
  const { message } = App.useApp()
  const [loans, setLoans] = useState<LoanType[]>([])
  const [loading, setLoading] = useState(true)

  async function load() {
    setLoading(true)
    try {
      setLoans(await getLoanTypes())
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

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
      <Table rowKey="id" loading={loading} columns={columns} dataSource={loans} pagination={false} />
    </Card>
  )
}

function AddEmployeeSection() {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)
  const [created, setCreated] = useState<{ username: string; temporaryPassword: string } | null>(null)
  const [positions, setPositions] = useState<JobPosition[]>([])
  const role = Form.useWatch('role', form)

  useEffect(() => {
    getJobPositions(true).then(setPositions).catch(() => {})
  }, [])

  async function onFinish(values: {
    firstName: string
    lastName: string
    personnelNumber: string
    username: string
    hireDate: { toISOString: () => string }
    role: string
    jobPositionId?: number
  }) {
    setSubmitting(true)
    try {
      const res = await createEmployee({
        firstName: values.firstName,
        lastName: values.lastName,
        personnelNumber: values.personnelNumber,
        username: values.username,
        hireDate: values.hireDate.toISOString(),
        role: values.role,
        jobPositionId: values.jobPositionId,
      })
      setCreated({ username: res.username, temporaryPassword: res.temporaryPassword })
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
            message="رمز عبور لازم نیست وارد کنید"
            description="سیستم یک رمز موقت امن به‌صورت خودکار می‌سازد و بعد از ایجاد کاربر همین‌جا نمایش می‌دهد. کاربر بعد از اولین ورود می‌تواند رمزش را از بخش «اطلاعات کاربری» تغییر دهد."
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
                <div>
                  رمز موقت: <b style={{ fontFamily: 'monospace' }}>{created.temporaryPassword}</b>
                </div>
                <div style={{ marginTop: 8, color: '#c41d7f' }}>
                  این رمز فقط یک‌بار نمایش داده می‌شود؛ آن را به کاربر اطلاع دهید.
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
  role: string
  isActive: boolean
  jobPositionTitle?: string | null
  effectiveMonthlySalary: number
  maxMonthlyInstallment: number
  employmentStatus: string
}

function PeopleSection({ role, title }: { role: 'Admin' | 'Employee'; title: string }) {
  const [rows, setRows] = useState<EmployeeRow[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setLoading(true)
    getAllEmployees()
      .then((data) => setRows(Array.isArray(data) ? data : data.items ?? []))
      .finally(() => setLoading(false))
  }, [])

  // فقط افراد با نقش موردنظر همین بخش نمایش داده می‌شوند.
  const filtered = rows.filter((r) => r.role === role)

  const money = (v: number) => (v > 0 ? `${v.toLocaleString('fa-IR')} تومان` : '—')

  const columns: ColumnsType<EmployeeRow> = [
    { title: 'نام', render: (_, r) => `${r.firstName} ${r.lastName}` },
    { title: 'نام کاربری', dataIndex: 'username' },
    { title: 'شماره پرسنلی', dataIndex: 'personnelNumber' },
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
    {
      title: 'حساب کاربری',
      dataIndex: 'isActive',
      render: (v: boolean) => (v ? <Tag color="green">فعال</Tag> : <Tag>غیرفعال</Tag>),
    },
  ]

  return (
    <Card title={`${title} (${filtered.length})`}>
      <Table rowKey="id" loading={loading} columns={columns} dataSource={filtered} pagination={{ pageSize: 10 }} />
    </Card>
  )
}
