import { useState } from 'react'
import { Button, Avatar, Upload, Popconfirm, Modal, App } from 'antd'
import {
  UserOutlined,
  DeleteOutlined,
  LogoutOutlined,
  UploadOutlined,
} from '@ant-design/icons'
import { useAuth } from '../auth/AuthContext'
import { uploadProfilePicture, deleteProfilePicture } from '../api/services'

/**
 * پنل پروفایل مشترک بین داشبورد کارمند و ادمین.
 *
 * ویرایش پروفایل (رمز، ایمیل، شماره‌ها، عکس) برای کارمند و ادمین یکسان است و
 * از همان اندپوینت /auth/profile می‌آید.
 */
export function ProfilePanel() {
  const { user, refreshUser, logout } = useAuth()
  const { message } = App.useApp()
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
