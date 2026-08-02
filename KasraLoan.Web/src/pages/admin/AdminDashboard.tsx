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
  App,
} from 'antd'
import {
  FileProtectOutlined,
  BankOutlined,
  UserAddOutlined,
  TeamOutlined,
  CrownOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { DashboardLayout } from '../../components/DashboardLayout'
import {
  getAllPermissionRequests,
  approvePermissionRequest,
  rejectPermissionRequest,
  getLoanTypes,
  setLoanTypeStatus,
  createEmployee,
  getAllEmployees,
} from '../../api/services'
import type { LoanType, LoanPermissionRequestItem } from '../../api/types'

const statusTag: Record<string, { color: string; label: string }> = {
  Pending: { color: 'gold', label: 'در انتظار' },
  Approved: { color: 'green', label: 'تأیید شده' },
  Rejected: { color: 'red', label: 'رد شده' },
}

export function AdminDashboard() {
  const [section, setSection] = useState('permissions')

  const menuItems = [
    { key: 'permissions', icon: <FileProtectOutlined />, label: 'درخواست‌های مجوز' },
    { key: 'loans', icon: <BankOutlined />, label: 'مدیریت وام‌ها' },
    { key: 'addEmployee', icon: <UserAddOutlined />, label: 'افزودن کاربر' },
    { key: 'employees', icon: <TeamOutlined />, label: 'کارمندان' },
    { key: 'admins', icon: <CrownOutlined />, label: 'ادمین‌ها' },
  ]

  return (
    <DashboardLayout
      title="داشبورد مدیریت"
      menuItems={menuItems}
      selectedKey={section}
      onSelect={setSection}
    >
      {section === 'permissions' && <PermissionRequestsSection />}
      {section === 'loans' && <LoanManagementSection />}
      {section === 'addEmployee' && <AddEmployeeSection />}
      {section === 'employees' && <PeopleSection role="Employee" title="لیست کارمندان" />}
      {section === 'admins' && <PeopleSection role="Admin" title="لیست ادمین‌ها" />}
    </DashboardLayout>
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

  async function onFinish(values: {
    firstName: string
    lastName: string
    personnelNumber: string
    username: string
    hireDate: { toISOString: () => string }
    role: string
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
            <Form.Item label="شماره پرسنلی" name="personnelNumber" rules={[{ required: true }]}>
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

  const columns: ColumnsType<EmployeeRow> = [
    { title: 'نام', render: (_, r) => `${r.firstName} ${r.lastName}` },
    { title: 'نام کاربری', dataIndex: 'username' },
    { title: 'شماره پرسنلی', dataIndex: 'personnelNumber' },
    {
      title: 'وضعیت',
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
