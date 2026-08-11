import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Row,
  Col,
  Card,
  Tag,
  Button,
  Form,
  Select,
  Input,
  Table,
  Empty,
  App,
  Alert,
  Avatar,
  Upload,
  Popconfirm,
  Modal,
  Segmented,
  Badge,
  List,
  Drawer,
  InputNumber,
  Descriptions,
  Progress,
  DatePicker,
} from 'antd'
import {
  BankOutlined,
  UserOutlined,
  PlusOutlined,
  DeleteOutlined,
  ArrowRightOutlined,
  LogoutOutlined,
  UploadOutlined,
  BellOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { DashboardLayout } from '../../components/DashboardLayout'
import { useAuth } from '../../auth/AuthContext'
import {
  getLoanTypes,
  getMyPermissionRequests,
  createPermissionRequest,
  getMyLoans,
  updateProfile,
  uploadProfilePicture,
  deleteProfilePicture,
  getUnreadCount,
  getMyNotifications,
  markAllNotificationsRead,
  createLoanRequest,
  getLoanInstallments,
  getLoanOutstanding,
  payInstallment,
  getCurrentInstallment,
  selectPaymentMethod,
  startGatewayPayment,
  submitCheque,
} from '../../api/services'
import type {
  LoanType,
  LoanPermissionRequestItem,
  MyLoanItem,
  NotificationItem,
  LoanInstallment,
  LoanOutstanding,
  CurrentInstallment,
} from '../../api/types'

const MIN_SCORE = 600

/**
 * کارمزد سالانه‌ی هر نوع وام، برای تخمین قسط در فرم — همان اعدادی که در
 * قوانین بک‌اند هستند. اگر آن‌جا عوض شد، اینجا هم باید عوض شود؛ تا وقتی
 * قوانین از دیتابیس خوانده نشوند راه بهتری نیست.
 */
const LOAN_FEE_PERCENT: Record<string, number> = {
  TravelLoan: 0,
  SpecialCaseLoan: 4,
  MarriageLoan: 5,
  ImmediatePaymentLoan: 2,
}

const statusTag: Record<string, { color: string; label: string }> = {
  Pending: { color: 'gold', label: 'در انتظار بررسی' },
  Approved: { color: 'green', label: 'تأیید شده' },
  Rejected: { color: 'red', label: 'رد شده' },
}

const loanStatusMap: Record<string, { color: string; label: string }> = {
  Pending: { color: 'gold', label: 'در انتظار' },
  Approved: { color: 'green', label: 'تأیید شده' },
  Rejected: { color: 'red', label: 'رد شده' },
  Active: { color: 'blue', label: 'فعال' },
  Paid: { color: 'green', label: 'تسویه شده' },
  Closed: { color: 'default', label: 'بسته شده' },
}

const LOAN_SECTIONS = ['loans', 'permission', 'loanHistory', 'installments']

export function EmployeeDashboard() {
  const [section, setSection] = useState('welcome')
  const [unread, setUnread] = useState(0)
  const [profileOpen, setProfileOpen] = useState(false)

  useEffect(() => {
    getUnreadCount().then(setUnread).catch(() => {})
    const timer = setInterval(() => getUnreadCount().then(setUnread).catch(() => {}), 30000)
    return () => clearInterval(timer)
  }, [])

  const menuItems = [
    { key: 'loans', icon: <BankOutlined />, label: 'وام' },
    {
      key: 'notifications',
      icon: (
        <Badge count={unread} size="small" offset={[-2, 2]}>
          <BellOutlined style={{ fontSize: 22 }} />
        </Badge>
      ),
      label: 'اعلان',
    },
  ]

  // آیتم «وام» در نوار کناری وقتی هر کدام از زیربخش‌های وام باز است، انتخاب‌شده می‌ماند.
  const selectedKey = LOAN_SECTIONS.includes(section) ? 'loans' : section

  function handleSelect(key: string) {
    setSection(key)
    if (key === 'notifications') {
      markAllNotificationsRead()
        .then(() => setUnread(0))
        .catch(() => {})
    }
  }

  return (
    <DashboardLayout
      menuItems={menuItems}
      selectedKey={selectedKey}
      onSelect={handleSelect}
      hideLogout
      rail
      onAvatarClick={() => setProfileOpen(true)}
    >
      {section === 'welcome' && <WelcomeSection />}

      {LOAN_SECTIONS.includes(section) && (
        <div>
          <Segmented
            value={section}
            onChange={(v) => setSection(v as string)}
            options={[
              { label: 'درخواست وام', value: 'loans' },
              { label: 'درخواست مجوز وام', value: 'permission' },
              { label: 'اقساط من', value: 'installments' },
              { label: 'سابقه وام', value: 'loanHistory' },
            ]}
            style={{ marginBottom: 16 }}
          />
          {section === 'loans' && <LoansSection />}
          {section === 'permission' && <PermissionSection />}
          {section === 'installments' && <InstallmentsSection />}
          {section === 'loanHistory' && <LoanHistorySection />}
        </div>
      )}

      {section === 'notifications' && <NotificationsSection />}

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
        <ProfileSection />
      </Drawer>
    </DashboardLayout>
  )
}

function WelcomeSection() {
  const { user } = useAuth()

  return (
    <div style={{ display: 'grid', placeItems: 'center', minHeight: '60vh' }}>
      <Card style={{ textAlign: 'center', maxWidth: 520, width: '100%' }}>
        <div style={{ fontSize: 56, marginBottom: 12 }}>👋</div>
        <h2 style={{ margin: '0 0 8px' }}>
          کاربر {user?.firstName} {user?.lastName}، خوش آمدید
        </h2>
        <p style={{ color: 'var(--text-muted)', margin: 0 }}>
          به سامانه‌ی صندوق همیار کسرا خوش آمدید. برای شروع، از منوی سمت راست بخش موردنظر را انتخاب کنید.
        </p>
      </Card>
    </div>
  )
}

function NotificationsSection() {
  const [items, setItems] = useState<NotificationItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getMyNotifications()
      .then((d) => setItems(d.items))
      .finally(() => setLoading(false))
  }, [])

  return (
    <Card title="اعلان‌ها">
      <List
        loading={loading}
        dataSource={items}
        locale={{ emptyText: <Empty description="اعلانی نداری" /> }}
        renderItem={(n) => (
          <List.Item>
            <List.Item.Meta
              title={<span style={{ fontWeight: 600 }}>{n.title}</span>}
              description={n.message}
            />
            <span style={{ color: 'var(--text-muted)', fontSize: 12, whiteSpace: 'nowrap' }}>
              {new Date(n.createdAt).toLocaleDateString('fa-IR')}
            </span>
          </List.Item>
        )}
      />
    </Card>
  )
}

function LoansSection() {
  const { user } = useAuth()
  const [loans, setLoans] = useState<LoanType[]>([])
  const [loading, setLoading] = useState(true)
  const [selected, setSelected] = useState<LoanType | null>(null)

  useEffect(() => {
    getLoanTypes()
      .then(setLoans)
      .finally(() => setLoading(false))
  }, [])

  const scoreOk = (user?.score ?? 0) >= MIN_SCORE
  const employed = user?.employmentStatus !== 'Terminated'

  return (
    <>
      {!employed && (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message="وضعیت اشتغال شما فعال نیست."
          description="امکان ثبت درخواست وام جدید وجود ندارد، اما همچنان می‌توانید اقساط وام‌های قبلی را ببینید و پرداخت کنید."
        />
      )}
      {employed && !scoreOk && (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message={`امتیاز شما ${user?.score ?? 0} است و برای دریافت وام حداقل ${MIN_SCORE} لازم است.`}
          description="می‌توانید از بخش «درخواست مجوز وام» درخواست استثنا ثبت کنید."
        />
      )}
      {employed && scoreOk && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message={
            <span>
              سمت شما «{user?.jobPositionTitle ?? '—'}» است و سقف قسط ماهانه‌تان{' '}
              <b>{(user?.maxMonthlyInstallment ?? 0).toLocaleString('fa-IR')} تومان</b> است
              (یک‌سوم حقوق).
            </span>
          }
          description="مبلغ وامی که می‌توانید بگیرید به همین سقف و تعداد اقساط انتخابی بستگی دارد."
        />
      )}

      <Row gutter={[16, 16]}>
        {loans.map((loan) => (
          <Col xs={24} sm={12} lg={8} key={loan.id}>
            <Card
              loading={loading}
              title={loan.name}
              extra={
                loan.isActive ? (
                  <Tag color="green">فعال</Tag>
                ) : (
                  <Tag color="default">غیرفعال</Tag>
                )
              }
            >
              {!loan.isActive ? (
                <Alert type="error" message="این وام در حال حاضر غیرفعال است." />
              ) : !employed ? (
                <Button block disabled>
                  وضعیت اشتغال فعال نیست
                </Button>
              ) : scoreOk ? (
                <Button type="primary" block onClick={() => setSelected(loan)}>
                  درخواست این وام
                </Button>
              ) : (
                <Button block disabled>
                  امتیاز کافی نیست
                </Button>
              )}
            </Card>
          </Col>
        ))}
      </Row>

      <LoanRequestModal
        loanType={selected}
        onClose={() => setSelected(null)}
      />
    </>
  )
}

/**
 * فرم درخواست وام.
 *
 * سقف مبلغ همین‌جا و پیش از ارسال تخمین زده می‌شود تا کارمند قبل از خوردن به
 * خطای سرور بداند چه چیزی ممکن است. مرجع نهایی همچنان بک‌اند است — این فقط
 * راهنماست، نه جایگزین قانون.
 */
function LoanRequestModal({
  loanType,
  onClose,
}: {
  loanType: LoanType | null
  onClose: () => void
}) {
  const { user } = useAuth()
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)
  const [amount, setAmount] = useState<number>(0)
  const [months, setMonths] = useState<number>(12)

  useEffect(() => {
    if (loanType) {
      form.resetFields()
      setAmount(0)
      setMonths(12)
    }
  }, [loanType, form])

  if (!loanType) return null

  const cap = user?.maxMonthlyInstallment ?? 0

  // همان فرمول بک‌اند: کارمزد سالانه‌ی ساده روی اصل مبلغ.
  const feeRate = (LOAN_FEE_PERCENT[loanType.type] ?? 0) / 100
  const feeMultiplier = 1 + feeRate * (months / 12)

  const maxBySalary = cap > 0 ? Math.floor((cap * months) / feeMultiplier) : 0
  const estimatedTotal = Math.round(amount * feeMultiplier)
  const estimatedMonthly = months > 0 ? Math.round(estimatedTotal / months) : 0
  const overCap = amount > 0 && estimatedMonthly > cap

  async function onFinish(values: { requestedAmount: number; installmentCount: number }) {
    setSubmitting(true)
    try {
      await createLoanRequest({
        loanTypeId: loanType!.id,
        requestedAmount: values.requestedAmount,
        installmentCount: values.installmentCount,
      })
      message.success('درخواست وام ثبت شد و در انتظار بررسی ادمین است.')
      onClose()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ثبت درخواست وام.')
    } finally {
      setSubmitting(false)
    }
  }

  const money = (v: number) => `${Math.max(0, v).toLocaleString('fa-IR')} تومان`

  return (
    <Modal
      open
      onCancel={onClose}
      footer={null}
      title={`درخواست ${loanType.name}`}
      destroyOnHidden
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        initialValues={{ installmentCount: 12 }}
      >
        <Form.Item
          label="مبلغ درخواستی (تومان)"
          name="requestedAmount"
          rules={[
            { required: true, message: 'مبلغ را وارد کنید' },
            {
              type: 'number',
              min: 1_000_000,
              message: 'مبلغ باید حداقل ۱,۰۰۰,۰۰۰ تومان باشد',
            },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            step={1_000_000}
            controls={false}
            formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={(v) => Number((v ?? '').replace(/,/g, ''))}
            onChange={(v) => setAmount(Number(v) || 0)}
            placeholder="مثلاً 100000000"
          />
        </Form.Item>

        <Form.Item
          label="تعداد اقساط"
          name="installmentCount"
          rules={[{ required: true, message: 'تعداد اقساط را انتخاب کنید' }]}
        >
          <Select
            onChange={(v) => setMonths(Number(v))}
            options={[6, 12, 18, 24, 36].map((m) => ({
              value: m,
              label: `${m.toLocaleString('fa-IR')} قسط`,
            }))}
          />
        </Form.Item>

        <Card size="small" style={{ marginBottom: 16, background: 'rgba(0,0,0,0.02)' }}>
          <Row gutter={[8, 8]}>
            <Col span={12}>سقف قسط ماهانه شما:</Col>
            <Col span={12} style={{ textAlign: 'left', fontWeight: 600 }}>{money(cap)}</Col>

            <Col span={12}>بیشترین وام در {months.toLocaleString('fa-IR')} قسط:</Col>
            <Col span={12} style={{ textAlign: 'left', fontWeight: 600 }}>{money(maxBySalary)}</Col>

            {amount > 0 && (
              <>
                <Col span={12}>کل بازپرداخت:</Col>
                <Col span={12} style={{ textAlign: 'left' }}>{money(estimatedTotal)}</Col>

                <Col span={12}>قسط ماهانه:</Col>
                <Col
                  span={12}
                  style={{ textAlign: 'left', fontWeight: 700, color: overCap ? '#cf1322' : '#389e0d' }}
                >
                  {money(estimatedMonthly)}
                </Col>
              </>
            )}
          </Row>
        </Card>

        {overCap && (
          <Alert
            type="warning"
            showIcon
            style={{ marginBottom: 16 }}
            message="قسط از سقف حقوق شما بیشتر است"
            description={`مبلغ را کمتر کنید یا تعداد اقساط را بالا ببرید. با ${months.toLocaleString('fa-IR')} قسط، حداکثر ${money(maxBySalary)} می‌توانید بگیرید.`}
          />
        )}

        <Button type="primary" htmlType="submit" block loading={submitting}>
          ثبت درخواست
        </Button>
      </Form>
    </Modal>
  )
}

function PermissionSection() {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [loanTypes, setLoanTypes] = useState<LoanType[]>([])
  const [requests, setRequests] = useState<LoanPermissionRequestItem[]>([])
  const [submitting, setSubmitting] = useState(false)

  async function loadRequests() {
    setRequests(await getMyPermissionRequests())
  }

  useEffect(() => {
    getLoanTypes(true).then(setLoanTypes)
    loadRequests()
  }, [])

  async function onFinish(values: { loanTypeId: number; reason: string }) {
    setSubmitting(true)
    try {
      await createPermissionRequest(values.loanTypeId, values.reason)
      message.success('درخواست مجوز با موفقیت ثبت شد.')
      form.resetFields()
      await loadRequests()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ثبت درخواست.')
    } finally {
      setSubmitting(false)
    }
  }

  const columns: ColumnsType<LoanPermissionRequestItem> = [
    { title: 'نوع وام', dataIndex: 'loanTypeName' },
    { title: 'دلیل', dataIndex: 'reason', ellipsis: true },
    {
      title: 'وضعیت',
      dataIndex: 'status',
      render: (s: string) => (
        <Tag color={statusTag[s]?.color}>{statusTag[s]?.label ?? s}</Tag>
      ),
    },
    {
      title: 'تاریخ',
      dataIndex: 'createdAt',
      render: (d: string) => new Date(d).toLocaleDateString('fa-IR'),
    },
    {
      title: 'پاسخ ادمین',
      dataIndex: 'adminResponse',
      render: (r?: string) => r || '—',
    },
  ]

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} lg={9}>
        <Card title="ثبت درخواست مجوز وام">
          <Form form={form} layout="vertical" onFinish={onFinish}>
            <Form.Item
              label="نوع وام"
              name="loanTypeId"
              rules={[{ required: true, message: 'نوع وام را انتخاب کنید' }]}
            >
              <Select
                placeholder="انتخاب کنید"
                options={loanTypes.map((l) => ({ value: l.id, label: l.name }))}
              />
            </Form.Item>
            <Form.Item
              label="دلیل درخواست"
              name="reason"
              rules={[{ required: true, message: 'دلیل را بنویسید' }]}
            >
              <Input.TextArea rows={4} placeholder="چرا به این وام نیاز دارید؟" />
            </Form.Item>
            <Button type="primary" htmlType="submit" block loading={submitting}>
              ارسال درخواست
            </Button>
          </Form>
        </Card>
      </Col>
      <Col xs={24} lg={15}>
        <Card title="درخواست‌های من">
          <Table
            rowKey="id"
            columns={columns}
            dataSource={requests}
            locale={{ emptyText: <Empty description="درخواستی ثبت نکرده‌ای" /> }}
            pagination={{ pageSize: 5 }}
          />
        </Card>
      </Col>
    </Row>
  )
}

/**
 * اقساط وام‌های فعال کارمند، همراه با مانده و دکمه‌ی پرداخت.
 * فقط وام‌هایی که اقساط دارند (تأییدشده یا فعال) نمایش داده می‌شوند؛
 * وام در انتظار بررسی هنوز قسطی ندارد.
 */
function InstallmentsSection() {
  const { message } = App.useApp()
  const [loans, setLoans] = useState<MyLoanItem[]>([])
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [installments, setInstallments] = useState<LoanInstallment[]>([])
  const [outstanding, setOutstanding] = useState<LoanOutstanding | null>(null)
  const [loading, setLoading] = useState(true)
  const [payingId, setPayingId] = useState<string | null>(null)
  const [current, setCurrent] = useState<CurrentInstallment | null>(null)

  const payable = loans.filter((l) => l.status === 'Approved' || l.status === 'Active')

  useEffect(() => {
    getMyLoans()
      .then((all) => {
        setLoans(all)
        const first = all.find((l) => l.status === 'Approved' || l.status === 'Active')
        if (first) setSelectedId(first.id)
      })
      .finally(() => setLoading(false))
  }, [])

  async function loadDetails(loanId: string) {
    const [inst, out, cur] = await Promise.all([
      getLoanInstallments(loanId),
      getLoanOutstanding(loanId),
      getCurrentInstallment(),
    ])
    setInstallments(inst.sort((a, b) => a.installmentNumber - b.installmentNumber))
    setOutstanding(out)
    setCurrent(cur)
  }

  useEffect(() => {
    if (selectedId) loadDetails(selectedId).catch(() => {})
  }, [selectedId])

  async function pay(installmentId: string) {
    setPayingId(installmentId)
    try {
      await payInstallment(installmentId)
      message.success('قسط با موفقیت پرداخت شد.')
      if (selectedId) await loadDetails(selectedId)
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در پرداخت قسط.')
    } finally {
      setPayingId(null)
    }
  }

  const money = (v: number) => `${v.toLocaleString('fa-IR')} تومان`

  const columns: ColumnsType<LoanInstallment> = [
    { title: 'شماره قسط', dataIndex: 'installmentNumber', width: 100 },
    { title: 'مبلغ', dataIndex: 'amount', render: money },
    {
      title: 'سررسید',
      dataIndex: 'dueDate',
      render: (d: string) => new Date(d).toLocaleDateString('fa-IR'),
    },
    {
      title: 'وضعیت',
      dataIndex: 'isPaid',
      render: (paid: boolean) =>
        paid ? <Tag color="green">پرداخت شده</Tag> : <Tag color="gold">پرداخت نشده</Tag>,
    },
    {
      title: 'عملیات',
      render: (_, row) =>
        row.isPaid ? (
          <span style={{ color: 'var(--text-muted)' }}>—</span>
        ) : row.id === current?.loanInstallmentId ? (
          <Tag color="blue">قسط جاری</Tag>
        ) : (
          <Popconfirm
            title="پرداخت قسط"
            description={`مبلغ ${money(row.amount)} پرداخت شود؟`}
            okText="بله، پرداخت کن"
            cancelText="انصراف"
            onConfirm={() => pay(row.id)}
          >
            <Button size="small" loading={payingId === row.id}>
              پرداخت
            </Button>
          </Popconfirm>
        ),
    },
  ]

  if (loading) return <Card loading />

  if (payable.length === 0) {
    return (
      <Card>
        <Empty description="وام فعالی نداری که قسط داشته باشد" />
      </Card>
    )
  }

  const progress = outstanding && outstanding.totalInstallments > 0
    ? Math.round((outstanding.paidInstallments / outstanding.totalInstallments) * 100)
    : 0

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24}>
        {current?.hasDueInstallment && (
          <PaymentMethodPanel
            current={current}
            onChanged={() => selectedId && loadDetails(selectedId)}
          />
        )}
      </Col>

      <Col xs={24} lg={8}>
        <Card title="وام‌های فعال">
          <Select
            style={{ width: '100%', marginBottom: 16 }}
            value={selectedId}
            onChange={setSelectedId}
            options={payable.map((l) => ({
              value: l.id,
              label: `${l.loanType} — ${money(l.approvedAmount)}`,
            }))}
          />

          {outstanding && (
            <>
              <Progress percent={progress} status={progress === 100 ? 'success' : 'active'} />
              <Descriptions column={1} size="small" style={{ marginTop: 12 }}>
                <Descriptions.Item label="کل بازپرداخت">
                  {money(outstanding.totalPayableAmount)}
                </Descriptions.Item>
                <Descriptions.Item label="پرداخت‌شده">
                  {money(outstanding.paidAmount)}
                </Descriptions.Item>
                <Descriptions.Item label="مانده">
                  <b>{money(outstanding.outstandingAmount)}</b>
                </Descriptions.Item>
                <Descriptions.Item label="اقساط باقی‌مانده">
                  {outstanding.remainingInstallments.toLocaleString('fa-IR')} از{' '}
                  {outstanding.totalInstallments.toLocaleString('fa-IR')}
                </Descriptions.Item>
              </Descriptions>

              {outstanding.isSettlementDemanded && (
                <Alert
                  type="error"
                  showIcon
                  style={{ marginTop: 12 }}
                  message="تسویه‌ی یکجا مطالبه شده است"
                  description={
                    <>
                      کل مانده باید تا تاریخ <b>{outstanding.settlementDueDatePersian}</b> پرداخت
                      شود.
                      {outstanding.settlementReason && <div>دلیل: {outstanding.settlementReason}</div>}
                    </>
                  }
                />
              )}
            </>
          )}
        </Card>
      </Col>

      <Col xs={24} lg={16}>
        <Card title="اقساط">
          <Table
            rowKey="id"
            columns={columns}
            dataSource={installments}
            pagination={{ pageSize: 12 }}
            locale={{ emptyText: <Empty description="قسطی ثبت نشده" /> }}
          />
        </Card>
      </Col>
    </Row>
  )
}

const METHOD_LABEL: Record<string, string> = {
  PayrollDeduction: 'کسر از حقوق',
  OnlineGateway: 'پرداخت آنلاین',
  Cheque: 'چک',
}

const PAYMENT_STATUS_LABEL: Record<string, { color: string; label: string }> = {
  Selected: { color: 'blue', label: 'انتخاب شده' },
  AwaitingAdminApproval: { color: 'gold', label: 'در انتظار تأیید ادمین' },
  Confirmed: { color: 'green', label: 'پرداخت شده' },
  Rejected: { color: 'red', label: 'رد شده' },
  Failed: { color: 'red', label: 'ناموفق' },
}

/**
 * انتخاب روش پرداخت برای قسط جاری.
 *
 * وقتی پنجره بسته است، گزینه‌ها غیرفعال می‌شوند و صراحتاً گفته می‌شود که قسط
 * از حقوق کسر خواهد شد — تا کارمند غافلگیر نشود.
 */
function PaymentMethodPanel({
  current,
  onChanged,
}: {
  current: CurrentInstallment
  onChanged: () => void
}) {
  const { message } = App.useApp()
  const navigate = useNavigate()
  const [busy, setBusy] = useState(false)
  const [chequeOpen, setChequeOpen] = useState(false)

  const installmentId = current.loanInstallmentId!
  const money = (v: number) => `${v.toLocaleString('fa-IR')} تومان`

  const locked =
    current.paymentStatus === 'AwaitingAdminApproval' ||
    current.paymentStatus === 'Confirmed'

  const disabled = !current.isSelectionWindowOpen || locked

  async function choose(method: 'PayrollDeduction' | 'OnlineGateway') {
    setBusy(true)
    try {
      if (method === 'OnlineGateway') {
        const session = await startGatewayPayment(installmentId)
        navigate(session.redirectUrl)
        return
      }

      await selectPaymentMethod(installmentId, method)
      message.success('روش پرداخت ثبت شد: کسر از حقوق')
      onChanged()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ثبت روش پرداخت.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <Card
      title={`قسط جاری — شماره ${current.installmentNumber.toLocaleString('fa-IR')}`}
      extra={
        current.selectedMethod ? (
          <Tag color={PAYMENT_STATUS_LABEL[current.paymentStatus ?? '']?.color}>
            {METHOD_LABEL[current.selectedMethod]} —{' '}
            {PAYMENT_STATUS_LABEL[current.paymentStatus ?? '']?.label ?? current.paymentStatus}
          </Tag>
        ) : (
          <Tag>روشی انتخاب نشده</Tag>
        )
      }
      style={{ marginBottom: 16 }}
    >
      <Row gutter={[16, 16]} align="middle">
        <Col xs={24} md={10}>
          <Descriptions column={1} size="small">
            <Descriptions.Item label="مبلغ">
              <b>{money(current.amount)}</b>
            </Descriptions.Item>
            <Descriptions.Item label="سررسید">{current.dueDatePersian}</Descriptions.Item>
          </Descriptions>
        </Col>

        <Col xs={24} md={14}>
          {locked ? (
            <Alert
              type={current.paymentStatus === 'Confirmed' ? 'success' : 'info'}
              showIcon
              message={
                current.paymentStatus === 'Confirmed'
                  ? 'این قسط پرداخت شده است.'
                  : 'چک شما در انتظار بررسی ادمین است؛ تا تعیین تکلیف نمی‌توانید روش را عوض کنید.'
              }
            />
          ) : !current.isSelectionWindowOpen ? (
            <Alert
              type="warning"
              showIcon
              message={`انتخاب روش پرداخت ${current.windowDescription} ممکن است.`}
              description="در صورت عدم انتخاب، این قسط به‌صورت خودکار از حقوق شما کسر می‌شود."
            />
          ) : (
            <Alert
              type="info"
              showIcon
              message="روش پرداخت این قسط را انتخاب کنید"
              description={`پنجره‌ی انتخاب ${current.windowDescription} باز است. اگر انتخابی نکنید، از حقوق کسر می‌شود.`}
            />
          )}

          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginTop: 12 }}>
            <Button
              type={current.selectedMethod === 'PayrollDeduction' ? 'primary' : 'default'}
              disabled={disabled || busy}
              onClick={() => choose('PayrollDeduction')}
            >
              کسر از حقوق
            </Button>
            <Button
              type="primary"
              disabled={disabled || busy}
              loading={busy}
              onClick={() => choose('OnlineGateway')}
            >
              پرداخت آنلاین
            </Button>
            <Button disabled={disabled || busy} onClick={() => setChequeOpen(true)}>
              ثبت چک
            </Button>
          </div>
        </Col>
      </Row>

      <ChequeModal
        open={chequeOpen}
        installmentId={installmentId}
        amount={current.amount}
        onClose={() => setChequeOpen(false)}
        onDone={() => {
          setChequeOpen(false)
          onChanged()
        }}
      />
    </Card>
  )
}

function ChequeModal({
  open,
  installmentId,
  amount,
  onClose,
  onDone,
}: {
  open: boolean
  installmentId: string
  amount: number
  onClose: () => void
  onDone: () => void
}) {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [file, setFile] = useState<File | null>(null)
  const [submitting, setSubmitting] = useState(false)

  async function onFinish(values: {
    chequeNumber: string
    chequeBankName: string
    chequeDate: { toISOString: () => string }
  }) {
    if (!file) {
      message.error('تصویر چک را انتخاب کنید.')
      return
    }

    setSubmitting(true)
    try {
      await submitCheque(
        installmentId,
        {
          chequeNumber: values.chequeNumber,
          chequeBankName: values.chequeBankName,
          chequeDate: values.chequeDate.toISOString(),
        },
        file,
      )
      message.success('چک ثبت شد و برای بررسی به ادمین ارسال گردید.')
      form.resetFields()
      setFile(null)
      onDone()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ثبت چک.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Modal open={open} onCancel={onClose} footer={null} title="ثبت چک" destroyOnHidden>
      <Alert
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
        message={`مبلغ چک باید ${amount.toLocaleString('fa-IR')} تومان باشد.`}
        description="چک تا زمان تأیید ادمین پرداخت‌شده محسوب نمی‌شود. اگر تا قطعی شدن لیست حقوق تأیید نشود، قسط از حقوق کسر می‌شود."
      />

      <Form form={form} layout="vertical" onFinish={onFinish}>
        <Form.Item
          label="شماره چک"
          name="chequeNumber"
          rules={[{ required: true, message: 'شماره چک را وارد کنید' }]}
        >
          <Input style={{ direction: 'ltr' }} />
        </Form.Item>

        <Form.Item
          label="بانک"
          name="chequeBankName"
          rules={[{ required: true, message: 'نام بانک را وارد کنید' }]}
        >
          <Input placeholder="مثلاً بانک ملت" />
        </Form.Item>

        <Form.Item
          label="تاریخ چک"
          name="chequeDate"
          rules={[{ required: true, message: 'تاریخ چک را انتخاب کنید' }]}
        >
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item label="تصویر چک" required>
          <Upload
            beforeUpload={(f) => {
              setFile(f)
              return false
            }}
            maxCount={1}
            accept="image/png,image/jpeg,image/webp"
            onRemove={() => setFile(null)}
          >
            <Button icon={<UploadOutlined />}>انتخاب تصویر</Button>
          </Upload>
        </Form.Item>

        <Button type="primary" htmlType="submit" block loading={submitting}>
          ارسال برای بررسی
        </Button>
      </Form>
    </Modal>
  )
}

function LoanHistorySection() {
  const [loans, setLoans] = useState<MyLoanItem[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getMyLoans()
      .then(setLoans)
      .finally(() => setLoading(false))
  }, [])

  const money = (v: number) => `${v.toLocaleString('fa-IR')} تومان`

  const columns: ColumnsType<MyLoanItem> = [
    { title: 'نوع وام', dataIndex: 'loanType' },
    { title: 'مبلغ درخواستی', dataIndex: 'requestedAmount', render: money },
    { title: 'مبلغ تأییدشده', dataIndex: 'approvedAmount', render: money },
    { title: 'تعداد اقساط', dataIndex: 'installmentCount' },
    {
      title: 'کل بازپرداخت',
      dataIndex: 'totalPayableAmount',
      // تا قبل از تأیید هنوز محاسبه نشده و صفر است.
      render: (v: number) => (v > 0 ? money(v) : '—'),
    },
    {
      title: 'قسط ماهانه',
      dataIndex: 'monthlyPaymentAmount',
      render: (v: number) => (v > 0 ? money(v) : '—'),
    },
    {
      title: 'وضعیت',
      dataIndex: 'status',
      render: (s: string) => (
        <Tag color={loanStatusMap[s]?.color}>{loanStatusMap[s]?.label ?? s}</Tag>
      ),
    },
  ]


  return (
    <Card title="سابقه وام‌های من">
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={loans}
        locale={{ emptyText: <Empty description="هنوز وامی نگرفته‌ای" /> }}
        pagination={{ pageSize: 8 }}
      />
    </Card>
  )
}

function ProfileSection() {
  const { user, refreshUser, logout } = useAuth()
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const [photoOpen, setPhotoOpen] = useState(false)

  if (!user) return null

  async function handleUpload(file: File) {
    try {
      await uploadProfilePicture(file)
      await refreshUser()
      message.success('عکس پروفایل به‌روزرسانی شد.')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در آپلود عکس.')
    }
  }

  async function handleDeletePhoto() {
    try {
      await deleteProfilePicture()
      await refreshUser()
      message.success('عکس پروفایل حذف شد.')
    } catch {
      message.error('خطا در حذف عکس.')
    }
  }

  async function onFinish(values: {
    additionalPhoneNumbers?: string[]
    email?: string
    newPassword?: string
  }) {
    setSaving(true)
    try {
      await updateProfile({
        additionalPhoneNumbers: (values.additionalPhoneNumbers ?? []).filter(
          (p) => p && p.trim() !== '',
        ),
        email: values.email,
        newPassword: values.newPassword || undefined,
      })
      await refreshUser()
      form.setFieldValue('newPassword', '')
      message.success('اطلاعات با موفقیت به‌روزرسانی شد.')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در به‌روزرسانی اطلاعات.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <>
      {/* عکس + نام + شماره پرسنلی */}
      <div style={{ textAlign: 'center', marginBottom: 24 }}>
        <div
          onClick={() => setPhotoOpen(true)}
          style={{ cursor: 'pointer', display: 'inline-block' }}
        >
          <Avatar size={110} src={user.profilePictureUrl || undefined} icon={<UserOutlined />} />
        </div>
        <div style={{ marginTop: 12, fontWeight: 700, fontSize: 18 }}>
          {user.firstName} {user.lastName}
        </div>
        <div style={{ color: 'var(--text-muted)', direction: 'ltr' }}>#{user.personnelNumber}</div>
      </div>

      {/* مودال عکس: آپلود و حذف بالای صفحه */}
      <Modal
        open={photoOpen}
        onCancel={() => setPhotoOpen(false)}
        footer={null}
        title={
          <div style={{ display: 'flex', gap: 8 }}>
            <Upload
              showUploadList={false}
              accept="image/png,image/jpeg,image/webp"
              beforeUpload={(file) => {
                handleUpload(file)
                setPhotoOpen(false)
                return false
              }}
            >
              <Button icon={<UploadOutlined />}>آپلود</Button>
            </Upload>
            <Popconfirm
              title="حذف عکس پروفایل"
              description="آیا از حذف عکس پروفایل مطمئن هستی؟"
              okText="بله، حذف کن"
              cancelText="انصراف"
              okButtonProps={{ danger: true }}
              onConfirm={() => {
                handleDeletePhoto()
                setPhotoOpen(false)
              }}
            >
              <Button danger icon={<DeleteOutlined />}>
                حذف
              </Button>
            </Popconfirm>
          </div>
        }
      >
        <div style={{ textAlign: 'center', padding: '12px 0' }}>
          {user.profilePictureUrl ? (
            <img
              src={user.profilePictureUrl}
              alt="profile"
              style={{ maxWidth: '100%', maxHeight: 360, borderRadius: 8 }}
            />
          ) : (
            <Avatar size={200} icon={<UserOutlined />} />
          )}
        </div>
      </Modal>

      <Form
            form={form}
            layout="vertical"
            onFinish={onFinish}
            onFinishFailed={(info) => {
              const first = info.errorFields?.[0]?.errors?.[0]
              message.error(first ?? 'لطفاً همه‌ی فیلدها را درست وارد کنید.')
            }}
            initialValues={{
              additionalPhoneNumbers: user.additionalPhoneNumbers ?? [],
              email: user.email ?? '',
            }}
          >
            <Row gutter={12}>
              <Col xs={24} sm={12} lg={24}>
                <Form.Item label="نام کاربری">
                  <Input value={user.username} disabled />
                </Form.Item>
              </Col>
            </Row>
              <Row>
                <Col xs={24} sm={12} lg={12}>
                <Form.Item
                  label="رمز عبور"
                  name="newPassword"
                  rules={[
                    { required: true, message: 'رمز عبور را وارد کنید' },
                    { min: 8, message: 'رمز عبور باید حداقل ۸ کاراکتر باشد' },
                    {
                      pattern: /^(?=.*[A-Za-z])(?=.*\d).+$/,
                      message: 'رمز عبور باید شامل حرف و عدد باشد',
                    },
                  ]}
                >
                  <Input.Password placeholder="رمز عبور" />
                </Form.Item>
              </Col>
              </Row>

            <Row gutter={12}>
              <Col xs={24} sm={12} lg={12}>
                <Form.Item label="امتیاز">
                  <Input value={String(user.score)} disabled />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12} lg={12}>
                <Form.Item
                  label="ایمیل"
                  name="email"
                  rules={[
                    { required: true, message: 'ایمیل را وارد کنید' },
                    { type: 'email', message: 'ایمیل معتبر نیست' },
                  ]}
                >
                  <Input placeholder="example@mail.com" />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item label="شماره تماس (اصلی)" style={{ maxWidth: 300 }}>
              <Input value={user.phoneNumber ?? '—'} disabled />
            </Form.Item>

            <Form.List name="additionalPhoneNumbers">
              {(fields, { add, remove }) => (
                <div style={{ marginBottom: 8, maxWidth: 340 }}>
                  <div style={{ marginBottom: 8, color: 'var(--text-muted)' }}>شماره‌های تماس اضافه (اختیاری)</div>
                  {fields.map(({ key, ...field }) => (
                    <div key={key} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                      <Form.Item
                        {...field}
                        rules={[
                          {
                            pattern: /^09\d{9}$/,
                            message: 'شماره باید مثل 09123456789 باشد',
                          },
                        ]}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input placeholder="مثلاً 09350000000" />
                      </Form.Item>
                      <Popconfirm
                        title="حذف شماره"
                        description="آیا از حذف این شماره مطمئن هستی؟"
                        okText="بله، حذف کن"
                        cancelText="انصراف"
                        okButtonProps={{ danger: true }}
                        onConfirm={() => remove(field.name)}
                      >
                        <Button danger icon={<DeleteOutlined />} />
                      </Popconfirm>
                    </div>
                  ))}
                  <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>
                    اضافه کردن شماره
                  </Button>
                </div>
              )}
            </Form.List>

            <Button type="primary" htmlType="submit" loading={saving}>
              ذخیره تغییرات
            </Button>
      </Form>
        <Button
          type="text"
          danger
          icon={<LogoutOutlined />}
          onClick={logout}
          style={{ paddingInline: 0 }}
        >
          خروج
        </Button>

    </>
  )
}
