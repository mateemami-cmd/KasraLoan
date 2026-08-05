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
} from 'antd'
import {
  BankOutlined,
  FileProtectOutlined,
  UserOutlined,
  HistoryOutlined,
  CameraOutlined,
  PlusOutlined,
  DeleteOutlined,
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
} from '../../api/services'
import type { LoanType, LoanPermissionRequestItem, MyLoanItem } from '../../api/types'

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

export function EmployeeDashboard() {
  const [section, setSection] = useState('welcome')

  const menuItems = [
    {
      key: 'loanGroup',
      icon: <BankOutlined />,
      label: 'وام',
      children: [
        { key: 'loans', icon: <BankOutlined />, label: 'درخواست وام' },
        { key: 'permission', icon: <FileProtectOutlined />, label: 'درخواست مجوز وام' },
        { key: 'loanHistory', icon: <HistoryOutlined />, label: 'سابقه وام' },
      ],
    },
    { key: 'profile', icon: <UserOutlined />, label: 'اطلاعات کاربری' },
  ]

  return (
    <DashboardLayout
      menuItems={menuItems}
      selectedKey={section}
      onSelect={setSection}
    >
      {section === 'welcome' && <WelcomeSection />}
      {section === 'loans' && <LoansSection />}
      {section === 'permission' && <PermissionSection />}
      {section === 'loanHistory' && <LoanHistorySection />}
      {section === 'profile' && <ProfileSection />}
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
        <p style={{ color: '#888', margin: 0 }}>
          به سامانه‌ی صندوق همیار کسرا خوش آمدید. برای شروع، از منوی سمت راست بخش موردنظر را انتخاب کنید.
        </p>
      </Card>
    </div>
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
  const { user, refreshUser } = useAuth()
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const [uploading, setUploading] = useState(false)

  if (!user) return null

  async function handleUpload(file: File) {
    setUploading(true)
    try {
      await uploadProfilePicture(file)
      await refreshUser()
      message.success('عکس پروفایل به‌روزرسانی شد.')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در آپلود عکس.')
    } finally {
      setUploading(false)
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
    <Card title="اطلاعات کاربری و ویرایش">
      <div style={{ textAlign: 'center', marginBottom: 24 }}>
        <Upload
          showUploadList={false}
          accept="image/png,image/jpeg,image/webp"
          beforeUpload={(file) => {
            handleUpload(file)
            return false
          }}
        >
            <div style={{ cursor: 'pointer', display: 'inline-block', position: 'relative' }}>
              <Avatar
                size={110}
                src={user.profilePictureUrl || undefined}
                icon={<UserOutlined />}
              />
              <div
                style={{
                  position: 'absolute',
                  insetInlineEnd: 4,
                  bottom: 4,
                  background: '#3d3f8c',
                  color: '#fff',
                  borderRadius: '50%',
                  width: 32,
                  height: 32,
                  display: 'grid',
                  placeItems: 'center',
                }}
              >
                <CameraOutlined />
              </div>
            </div>
          </Upload>
          {uploading && (
            <div style={{ marginTop: 10, color: '#888', fontSize: 12 }}>در حال آپلود...</div>
          )}
      </div>

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
                <Form.Item label="نام و نام خانوادگی">
                  <Input value={`${user.firstName} ${user.lastName}`} disabled />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12} lg={6}>
                <Form.Item label="شماره پرسنلی">
                  <Input value={user.personnelNumber} disabled />
                </Form.Item>
              </Col>
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
                  <div style={{ marginBottom: 8, color: '#555' }}>شماره‌های تماس اضافه (اختیاری)</div>
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
    </Card>
  )
}
