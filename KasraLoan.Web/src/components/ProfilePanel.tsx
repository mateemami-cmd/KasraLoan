import { useEffect, useState } from 'react'
import { Button, Upload, Image, App, Modal, Tag } from 'antd'
import { LogoutOutlined, PlusOutlined, LaptopOutlined } from '@ant-design/icons'
import type { UploadFile } from 'antd'
import { useAuth } from '../auth/AuthContext'
import {
  uploadProfilePicture,
  deleteProfilePicture,
  getSessions,
  revokeSession,
} from '../api/services'
import { SessionsTable } from './SessionsTable'
import type { SessionInfo } from '../api/types'

/**
 * پنل پروفایل مشترک بین داشبورد کارمند و ادمین.
 *
 * عکس پروفایل به‌صورت آپلودِ دایره‌ای (picture-circle) است: با hover روی عکس،
 * گزینه‌ی دیدن و حذف می‌آید و برای گذاشتن عکس جدید همان‌جا آپلود می‌شود — بدون
 * مودال جدا.
 */
export function ProfilePanel() {
  const { user, refreshUser, logout } = useAuth()
  const { message, modal } = App.useApp()
  const [previewOpen, setPreviewOpen] = useState(false)
  const [previewImage, setPreviewImage] = useState('')
  const [fileList, setFileList] = useState<UploadFile[]>([])
  const [sessionsOpen, setSessionsOpen] = useState(false)
  const [sessions, setSessions] = useState<SessionInfo[]>([])
  const [sessionsLoading, setSessionsLoading] = useState(false)
  const [revokingId, setRevokingId] = useState<number | null>(null)

  // فهرست فایل از روی عکس فعلیِ کاربر ساخته می‌شود و با هر تغییر همگام می‌ماند.
  useEffect(() => {
    setFileList(
      user?.profilePictureUrl
        ? [{ uid: '-1', name: 'profile', status: 'done', url: user.profilePictureUrl }]
        : [],
    )
  }, [user?.profilePictureUrl])

  if (!user) return null

  function handlePreview(file: UploadFile) {
    setPreviewImage(file.url || '')
    setPreviewOpen(true)
  }

  // beforeUpload همیشه false برمی‌گرداند تا آپلودِ خودکار antd انجام نشود؛
  // خودمان فایل را به سرور می‌فرستیم و کاربر را تازه می‌کنیم.
  async function beforeUpload(file: File) {
    try {
      await uploadProfilePicture(file)
      await refreshUser()
      message.success('عکس پروفایل به‌روزرسانی شد.')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در آپلود عکس.')
    }
    return false
  }

  // پیش از حذف، تأیید می‌گیریم. اگر کاربر «انصراف» بزند، عکس دست‌نخورده می‌ماند؛
  // فقط با تأیید حذف می‌شود. در هر حال false برمی‌گردانیم چون خودمان فهرست را با
  // refreshUser مدیریت می‌کنیم (نه حذفِ خودکارِ antd).
  function handleRemove() {
    return new Promise<boolean>((resolve) => {
      modal.confirm({
        title: 'حذف عکس پروفایل',
        content: 'آیا مطمئنید می‌خواهید عکس پروفایل را حذف کنید؟',
        okText: 'بله، حذف کن',
        okButtonProps: { danger: true },
        cancelText: 'انصراف',
        onOk: async () => {
          try {
            await deleteProfilePicture()
            await refreshUser()
            message.success('عکس پروفایل حذف شد.')
          } catch {
            message.error('خطا در حذف عکس.')
          }
          resolve(false)
        },
        onCancel: () => resolve(false),
      })
    })
  }

  async function openSessions() {
    setSessionsOpen(true)
    setSessionsLoading(true)
    try {
      setSessions(await getSessions())
    } catch {
      message.error('خطا در دریافت نشست‌ها.')
    } finally {
      setSessionsLoading(false)
    }
  }

  async function handleRevokeSession(id: number) {
    setRevokingId(id)
    try {
      await revokeSession(id)
      setSessions((prev) => prev.filter((s) => s.id !== id))
      message.success('نشست قطع شد.')
    } catch {
      message.error('خطا در قطع نشست.')
    } finally {
      setRevokingId(null)
    }
  }

  const uploadButton = (
    <button style={{ border: 0, background: 'none', cursor: 'pointer' }} type="button">
      <PlusOutlined />
      <div style={{ marginTop: 8 }}>آپلود</div>
    </button>
  )

  return (
    <>
      {/* عکسِ آپلودِ دایره‌ای سمت راست، نام + شماره پرسنلی کنارش */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 24 }}>
        <Upload
          listType="picture-circle"
          fileList={fileList}
          accept="image/png,image/jpeg,image/webp"
          onPreview={handlePreview}
          beforeUpload={beforeUpload}
          onRemove={handleRemove}
        >
          {fileList.length >= 1 ? null : uploadButton}
        </Upload>

        <div style={{ minWidth: 0 }}>
          <div style={{ fontWeight: 700, fontSize: 18 }}>
            {user.firstName} {user.lastName}
          </div>
          <div style={{ color: 'var(--text-muted)', direction: 'ltr', textAlign: 'right' }}>
            #{user.personnelNumber}
          </div>
        </div>
      </div>

      {previewImage && (
        <Image
          styles={{ root: { display: 'none' } }}
          preview={{
            open: previewOpen,
            onOpenChange: (visible) => setPreviewOpen(visible),
            afterOpenChange: (visible) => !visible && setPreviewImage(''),
          }}
          src={previewImage}
        />
      )}

      <Button
        type="text"
        icon={<LaptopOutlined />}
        onClick={openSessions}
        style={{ paddingInline: 0, marginTop: 8, display: 'block' }}
      >
        نشست‌های فعال
      </Button>

      <Button
        type="text"
        danger
        icon={<LogoutOutlined />}
        onClick={logout}
        style={{ paddingInline: 0, marginTop: 4 }}
      >
        خروج
      </Button>

      <Modal
        open={sessionsOpen}
        onCancel={() => setSessionsOpen(false)}
        footer={null}
        width={640}
        title="نشست‌های فعال"
      >
        <SessionsTable
          sessions={sessions}
          loading={sessionsLoading}
          renderAction={(s) =>
            s.isCurrent ? (
              <Tag color="blue">جاری</Tag>
            ) : (
              <Button
                danger
                size="small"
                loading={revokingId === s.id}
                onClick={() => handleRevokeSession(s.id)}
              >
                خروج
              </Button>
            )
          }
        />
      </Modal>
    </>
  )
}
