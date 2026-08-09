import { useEffect, useState } from 'react'
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
} from '../../api/services'
import type {
  LoanType,
  LoanPermissionRequestItem,
  MyLoanItem,
  NotificationItem,
} from '../../api/types'

const MIN_SCORE = 600

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

const LOAN_SECTIONS = ['loans', 'permission', 'loanHistory']

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
              { label: 'سابقه وام', value: 'loanHistory' },
            ]}
            style={{ marginBottom: 16 }}
          />
          {section === 'loans' && <LoansSection />}
          {section === 'permission' && <PermissionSection />}
          {section === 'loanHistory' && <LoanHistorySection />}
        </div>
      )}

      {section === 'notifications' && <NotificationsSection />}

      <Drawer
        title="پروفایل"
        placement="right"
        width={400}
        open={profileOpen}
        onClose={() => setProfileOpen(false)}
        closeIcon={<ArrowRightOutlined />}
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

  useEffect(() => {
    getLoanTypes()
      .then(setLoans)
      .finally(() => setLoading(false))
  }, [])

  const scoreOk = (user?.score ?? 0) >= MIN_SCORE

  return (
    <>
      {!scoreOk && (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message={`امتیاز شما ${user?.score ?? 0} است و برای دریافت وام حداقل ${MIN_SCORE} لازم است.`}
          description="می‌توانید از بخش «درخواست مجوز وام» درخواست استثنا ثبت کنید."
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
              ) : scoreOk ? (
                <Button type="primary" block>
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
    </>
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
    { title: 'مبلغ وام', dataIndex: 'requestedAmount', render: money },
    { title: 'تعداد اقساط', dataIndex: 'installmentCount' },
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
              <Col xs={24} sm={12} lg={6}>
                <Form.Item label="نام کاربری">
                  <Input value={user.username} disabled />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12} lg={6}>
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
              <Col xs={24} sm={12} lg={6}>
                <Form.Item label="امتیاز">
                  <Input value={String(user.score)} disabled />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12} lg={6}>
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

      <div style={{ borderTop: '1px solid var(--border-soft)', marginTop: 20, paddingTop: 12 }}>
        <Button
          type="text"
          danger
          icon={<LogoutOutlined />}
          onClick={logout}
          style={{ paddingInline: 0 }}
        >
          خروج
        </Button>
      </div>
    </>
  )
}
