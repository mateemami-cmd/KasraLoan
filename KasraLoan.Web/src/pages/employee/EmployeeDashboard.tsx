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
  Descriptions,
  Statistic,
  Empty,
  App,
  Alert,
} from 'antd'
import {
  BankOutlined,
  FileProtectOutlined,
  UserOutlined,
} from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { DashboardLayout } from '../../components/DashboardLayout'
import { useAuth } from '../../auth/AuthContext'
import {
  getLoanTypes,
  getMyPermissionRequests,
  createPermissionRequest,
} from '../../api/services'
import type { LoanType, LoanPermissionRequestItem } from '../../api/types'

const MIN_SCORE = 600

const statusTag: Record<string, { color: string; label: string }> = {
  Pending: { color: 'gold', label: 'در انتظار بررسی' },
  Approved: { color: 'green', label: 'تأیید شده' },
  Rejected: { color: 'red', label: 'رد شده' },
}

export function EmployeeDashboard() {
  const [section, setSection] = useState('loans')

  const menuItems = [
    { key: 'loans', icon: <BankOutlined />, label: 'وام‌ها' },
    { key: 'permission', icon: <FileProtectOutlined />, label: 'درخواست مجوز وام' },
    { key: 'profile', icon: <UserOutlined />, label: 'اطلاعات کاربری' },
  ]

  return (
    <DashboardLayout
      title="داشبورد کارمند"
      menuItems={menuItems}
      selectedKey={section}
      onSelect={setSection}
    >
      {section === 'loans' && <LoansSection />}
      {section === 'permission' && <PermissionSection />}
      {section === 'profile' && <ProfileSection />}
    </DashboardLayout>
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

function ProfileSection() {
  const { user } = useAuth()
  if (!user) return null

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} lg={8}>
        <Card>
          <Statistic title="امتیاز شما" value={user.score} suffix={`/ ${MIN_SCORE}`} />
        </Card>
      </Col>
      <Col xs={24} lg={16}>
        <Card title="اطلاعات کاربری">
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="نام">
              {user.firstName} {user.lastName}
            </Descriptions.Item>
            <Descriptions.Item label="نام کاربری">{user.username}</Descriptions.Item>
            <Descriptions.Item label="شماره پرسنلی">{user.personnelNumber}</Descriptions.Item>
            <Descriptions.Item label="تلفن">{user.phoneNumber || '—'}</Descriptions.Item>
            <Descriptions.Item label="ایمیل">{user.email || '—'}</Descriptions.Item>
          </Descriptions>
        </Card>
      </Col>
    </Row>
  )
}
