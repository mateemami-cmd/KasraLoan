import { useState } from 'react'
import { useNavigate, Navigate } from 'react-router-dom'
import { Button, Card, Form, Input, Typography, App, Spin } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useAuth } from '../auth/AuthContext'
import axios from 'axios'

export function LoginPage() {
  const { login, user, loading: authLoading } = useAuth()
  const navigate = useNavigate()
  const { message } = App.useApp()
  const [loading, setLoading] = useState(false)

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

  async function onFinish(values: { username: string; password: string }) {
    setLoading(true)
    try {
      const user = await login(values.username, values.password)
      message.success(`خوش آمدی ${user.firstName}!`)
      navigate(user.role === 'Admin' ? '/admin' : '/employee', { replace: true })
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
    </div>
  )
}
