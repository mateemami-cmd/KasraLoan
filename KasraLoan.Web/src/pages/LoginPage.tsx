import { useState } from 'react'
import { useNavigate, Navigate } from 'react-router-dom'
import { Button, Card, Form, Input, Typography, App, Spin, Modal, Alert } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useAuth } from '../auth/AuthContext'
import { SessionsTable } from '../components/SessionsTable'
import type { CurrentUser, SessionInfo } from '../api/types'
import axios from 'axios'

export function LoginPage() {
  const { login, user, loading: authLoading } = useAuth()
  const navigate = useNavigate()
  const { message } = App.useApp()
  const [loading, setLoading] = useState(false)
  // وقتی کاربر به سقفِ نشست‌ها می‌رسد، اطلاعاتِ ورود را نگه می‌داریم تا بعد از
  // انتخابِ نشستِ قطع‌شدنی، دوباره با همان اطلاعات تلاش کنیم.
  const [choice, setChoice] = useState<{
    username: string
    password: string
    sessions: SessionInfo[]
  } | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  // اگر کاربر از قبل واردشده باشد، اجازه نمی‌دهیم صفحه‌ی لاگین را ببیند و فوراً
  // به داشبوردش برمی‌گردانیم. این جلوی برگشتن با دکمه‌ی Back به صفحه‌ی ورود را
  // می‌گیرد؛ تنها راه دیدن دوباره‌ی این صفحه، «خروج» است که توکن را پاک می‌کند.
  if (authLoading) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', height: '100vh' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (user) {
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/employee'} replace />
  }

  function goToDashboard(u: CurrentUser) {
    message.success(`خوش آمدی ${u.firstName}!`)
    navigate(u.role === 'Admin' ? '/admin' : '/employee', { replace: true })
  }

  async function onFinish(values: { username: string; password: string }) {
    setLoading(true)
    try {
      const outcome = await login(values.username, values.password)
      if (outcome.status === 'choose') {
        // به سقفِ ۳ نشست رسیده؛ باید یکی را قطع کند.
        setChoice({ username: values.username, password: values.password, sessions: outcome.sessions })
        return
      }
      goToDashboard(outcome.user)
    } catch (err) {
      const msg =
        axios.isAxiosError(err) && err.response?.status === 401
          ? 'نام کاربری یا رمز عبور اشتباه است.'
          : 'خطا در ورود. لطفاً دوباره تلاش کنید.'
      message.error(msg)
    } finally {
      setLoading(false)
    }
  }

  // کاربر یکی از نشست‌ها را برای قطع انتخاب کرد؛ همان را می‌بندیم و دوباره وارد می‌شویم.
  async function handleTerminate(sessionId: number) {
    if (!choice) return
    setBusyId(sessionId)
    try {
      const outcome = await login(choice.username, choice.password, sessionId)
      if (outcome.status === 'choose') {
        setChoice({ ...choice, sessions: outcome.sessions })
        return
      }
      setChoice(null)
      goToDashboard(outcome.user)
    } catch {
      message.error('خطا در قطع نشست. دوباره تلاش کنید.')
    } finally {
      setBusyId(null)
    }
  }

  return (
    <div className="login-wrapper">
      <Card className="login-card" variant="borderless">
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Typography.Title level={3} style={{ marginBottom: 4 }}>
            صندوق همیار کسرا
          </Typography.Title>
          <Typography.Text type="secondary">سامانه مدیریت وام کارکنان</Typography.Text>
        </div>

        <Form layout="vertical" onFinish={onFinish} requiredMark={false} size="large">
          <Form.Item
            label="نام کاربری"
            name="username"
            rules={[{ required: true, message: 'نام کاربری را وارد کنید' }]}
          >
            <Input prefix={<UserOutlined />} placeholder="admin" />
          </Form.Item>

          <Form.Item
            label="رمز عبور"
            name="password"
            rules={[{ required: true, message: 'رمز عبور را وارد کنید' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="••••••••" />
          </Form.Item>

          <Button type="primary" htmlType="submit" block loading={loading}>
            ورود
          </Button>
        </Form>
      </Card>

      <Modal
        open={choice !== null}
        onCancel={() => setChoice(null)}
        footer={null}
        width={720}
        centered
        title="سقفِ دستگاه‌ها پر است"
      >
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message="روی ۳ دستگاه وارد هستید"
          description="برای ورود از این دستگاه، یکی از نشست‌های زیر را قطع کنید تا جا باز شود."
        />
        {choice && (
          <SessionsTable
            sessions={choice.sessions}
            renderAction={(s) => (
              <Button
                danger
                size="small"
                loading={busyId === s.id}
                onClick={() => handleTerminate(s.id)}
              >
                قطع و ورود
              </Button>
            )}
          />
        )}
      </Modal>
    </div>
  )
}
