import { useState } from 'react'
import { Button, Card, Form, Input, Typography, App, Divider } from 'antd'
import { LockOutlined } from '@ant-design/icons'
import { useAuth } from './AuthContext'
import { resetPassword } from '../api/services'

/**
 * صفحه‌ای که وقتی کاربر با «رمزِ موقتِ» فراموشیِ رمز وارد شده نمایش داده می‌شود:
 * تا رمزِ جدید نگذارد به داشبورد نمی‌رود. چون رمزِ فعلی موقت است و خودش نمی‌داند،
 * فقط رمزِ جدید و تکرارش گرفته می‌شود (نه رمزِ فعلی).
 */
export function ForcedResetPassword() {
  const { user, refreshUser, logout } = useAuth()
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)

  async function onFinish(values: { newPassword: string }) {
    setSubmitting(true)
    try {
      await resetPassword(values.newPassword)
      message.success('رمز عبور جدید با موفقیت تنظیم شد.')
      // با تازه‌سازیِ کاربر، mustResetPassword برابرِ false می‌شود و
      // ProtectedRoute خودش داشبورد را نشان می‌دهد.
      await refreshUser()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در تنظیم رمز عبور.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="login-wrapper">
      <Card className="login-card" variant="borderless">
        <div style={{ textAlign: 'center', marginBottom: 8 }}>
          <Typography.Title level={4} style={{ marginBottom: 4 }}>
            تعیین رمز عبور جدید
          </Typography.Title>
          <Typography.Text type="secondary">
            {user?.firstName} عزیز، با رمزِ موقت وارد شدید. برای ادامه یک رمزِ جدید بگذارید.
          </Typography.Text>
        </div>

        <Divider style={{ margin: '16px 0', borderColor: 'rgba(255,255,255,0.45)' }} />

        <Form form={form} layout="vertical" onFinish={onFinish} requiredMark={false} size="large">
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

          <Button type="primary" htmlType="submit" block loading={submitting}>
            ثبت رمز جدید
          </Button>
          <Button type="link" block onClick={logout} style={{ marginTop: 8 }}>
            انصراف و خروج
          </Button>
        </Form>
      </Card>
    </div>
  )
}
