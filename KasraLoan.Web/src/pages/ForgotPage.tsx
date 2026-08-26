import { useState } from 'react'
import { useNavigate, Navigate } from 'react-router-dom'
import { Button, Card, Form, Input, Typography, App, Alert, Spin } from 'antd'
import { UserOutlined, LockOutlined } from '@ant-design/icons'
import { useAuth } from '../auth/AuthContext'
import { verifyIdentity, resetByIdentity } from '../api/services'
import { hasTenDigits } from '../utils/nationalId'
import axios from 'axios'

/**
 * صفحه‌ی مستقلِ «فراموشی رمز عبور» (مسیر /Forgot). دو مرحله دارد: تأیید نام کاربری
 * + کد ملی، سپس تعیین رمز جدید. بعد از موفقیت به صفحه‌ی ورود برمی‌گردد.
 */
export function ForgotPage() {
  const navigate = useNavigate()
  const { user, loading: authLoading } = useAuth()
  const { message } = App.useApp()
  const [loading, setLoading] = useState(false)
  const [step, setStep] = useState<1 | 2>(1)
  const [creds, setCreds] = useState<{ username: string; nationalId: string } | null>(null)

  if (authLoading) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', height: '100vh' }}>
        <Spin size="large" />
      </div>
    )
  }

  // کاربرِ واردشده اینجا کاری ندارد؛ به داشبوردش می‌رود.
  if (user) {
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/employee'} replace />
  }

  function showError(err: unknown, fallback: string) {
    message.error(
      axios.isAxiosError(err) && err.response?.data?.message
        ? (err.response.data.message as string)
        : fallback,
    )
  }

  async function handleVerify(values: { username: string; nationalId: string }) {
    setLoading(true)
    try {
      await verifyIdentity(values.username, values.nationalId)
      setCreds({ username: values.username, nationalId: values.nationalId })
      setStep(2)
    } catch (err) {
      showError(err, 'خطا در بررسی اطلاعات.')
    } finally {
      setLoading(false)
    }
  }

  async function handleReset(values: { newPassword: string }) {
    if (!creds) return
    setLoading(true)
    try {
      await resetByIdentity(creds.username, creds.nationalId, values.newPassword)
      message.success('رمز عبور تغییر کرد. اکنون با رمز جدید وارد شوید.')
      navigate('/login', { replace: true })
    } catch (err) {
      showError(err, 'خطا در تغییر رمز عبور.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-wrapper">
      <Card className="login-card" variant="borderless">
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Typography.Title level={3} style={{ marginBottom: 4 }}>
            فراموشی رمز عبور
          </Typography.Title>
          <Typography.Text type="secondary">
            {step === 1
              ? 'نام کاربری و کد ملی خود را وارد کنید'
              : 'رمز عبور جدید را وارد کنید'}
          </Typography.Text>
        </div>

        {step === 1 ? (
          <Form layout="vertical" onFinish={handleVerify} requiredMark={false} size="large">
            <Form.Item
              label="نام کاربری"
              name="username"
              rules={[{ required: true, message: 'نام کاربری را وارد کنید' }]}
            >
              <Input prefix={<UserOutlined />} placeholder="نام کاربری" />
            </Form.Item>
            <Form.Item
              label="کد ملی"
              name="nationalId"
              rules={[
                { required: true, message: 'کد ملی را وارد کنید' },
                {
                  validator: (_, value) =>
                    !value || hasTenDigits(value)
                      ? Promise.resolve()
                      : Promise.reject(new Error('کد ملی باید دقیقاً ۱۰ رقم باشد')),
                },
              ]}
            >
              <Input
                placeholder="۱۰ رقم"
                maxLength={10}
                inputMode="numeric"
                style={{ direction: 'ltr', textAlign: 'right' }}
              />
            </Form.Item>
            <Button type="primary" htmlType="submit" block loading={loading}>
              ادامه
            </Button>
          </Form>
        ) : (
          <Form layout="vertical" onFinish={handleReset} requiredMark={false} size="large">
            <Alert
              type="success"
              showIcon
              style={{ marginBottom: 12 }}
              message="هویت تأیید شد. رمز عبور جدید را وارد کنید."
            />
            <Form.Item
              label="رمز عبور جدید"
              name="newPassword"
              rules={[
                { required: true, message: 'رمز عبور جدید را وارد کنید' },
                { min: 6, message: 'رمز عبور جدید باید حداقل ۶ کاراکتر باشد' },
              ]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="رمز عبور جدید را وارد کنید" />
            </Form.Item>
            <Form.Item
              label="تکرار رمز عبور جدید"
              name="confirmPassword"
              dependencies={['newPassword']}
              rules={[
                { required: true, message: 'رمز عبور جدید را دوباره وارد کنید' },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('newPassword') === value) return Promise.resolve()
                    return Promise.reject(new Error('دو رمز عبور یکسان نیستند'))
                  },
                }),
              ]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="تکرار رمز عبور جدید" />
            </Form.Item>
            <Button type="primary" htmlType="submit" block loading={loading}>
              ثبت رمز جدید
            </Button>
          </Form>
        )}

        <Button type="link" block onClick={() => navigate('/login')} style={{ marginTop: 8 }}>
          بازگشت به صفحه‌ی ورود
        </Button>
      </Card>
    </div>
  )
}
