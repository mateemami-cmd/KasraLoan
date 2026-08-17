import { useState } from 'react'
import {
  Row,
  Col,
  Form,
  Input,
  Button,
  Avatar,
  Upload,
  Popconfirm,
  Modal,
  App,
} from 'antd'
import {
  UserOutlined,
  PlusOutlined,
  DeleteOutlined,
  LogoutOutlined,
  UploadOutlined,
} from '@ant-design/icons'
import { useAuth } from '../auth/AuthContext'
import { updateProfile, uploadProfilePicture, deleteProfilePicture } from '../api/services'

/**
 * پنل پروفایل مشترک بین داشبورد کارمند و ادمین.
 *
 * ویرایش پروفایل (رمز، ایمیل، شماره‌ها، عکس) برای کارمند و ادمین یکسان است و
 * از همان اندپوینت /auth/profile می‌آید.
 */
export function ProfilePanel() {
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
          <Col xs={24} sm={12}>
            <Form.Item
              label="رمز عبور جدید"
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
          <Col xs={24} sm={12}>
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
              <div style={{ marginBottom: 8, color: 'var(--text-muted)' }}>
                شماره‌های تماس اضافه (اختیاری)
              </div>
              {fields.map(({ key, ...field }) => (
                <div key={key} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                  <Form.Item
                    {...field}
                    rules={[
                      { pattern: /^09\d{9}$/, message: 'شماره باید مثل 09123456789 باشد' },
                    ]}
                    style={{ flex: 1, marginBottom: 0 }}
                  >
                    <Input placeholder="مثلاً 09350000000" />
                  </Form.Item>
                  <Popconfirm
                    title="حذف شماره"
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
        style={{ paddingInline: 0, marginTop: 8 }}
      >
        خروج
      </Button>
    </>
  )
}
