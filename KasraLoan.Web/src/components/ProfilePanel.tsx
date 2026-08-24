import { useEffect, useState } from 'react'
import { Button, Upload, Image, App, Modal, Tag, Divider, Form, Input } from 'antd'
import {
  LogoutOutlined,
  PlusOutlined,
  DesktopOutlined,
  KeyOutlined,
  HistoryOutlined,
} from '@ant-design/icons'
import type { UploadFile } from 'antd'
import { useAuth } from '../auth/AuthContext'
import {
  uploadProfilePicture,
  deleteProfilePicture,
  changePassword,
  getSessions,
  revokeSession,
  getLoginHistory,
} from '../api/services'
import { SessionsTable } from './SessionsTable'
import { LoginHistoryTable } from './LoginHistoryTable'
import type { SessionInfo, LoginHistoryItem } from '../api/types'

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
  const [historyOpen, setHistoryOpen] = useState(false)
  const [loginHistory, setLoginHistory] = useState<LoginHistoryItem[]>([])
  const [historyLoading, setHistoryLoading] = useState(false)
  const [passwordOpen, setPasswordOpen] = useState(false)
  const [passwordSubmitting, setPasswordSubmitting] = useState(false)
  const [passwordForm] = Form.useForm()

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

  async function openLoginHistory() {
    setHistoryOpen(true)
    setHistoryLoading(true)
    try {
      setLoginHistory(await getLoginHistory())
    } catch {
      message.error('خطا در دریافت تاریخچه ورودها.')
    } finally {
      setHistoryLoading(false)
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

  function closePassword() {
    setPasswordOpen(false)
    passwordForm.resetFields()
  }

  async function submitPassword(values: { currentPassword: string; newPassword: string }) {
    setPasswordSubmitting(true)
    try {
      await changePassword(values.currentPassword, values.newPassword)
      message.success('رمز عبور با موفقیت تغییر کرد.')
      closePassword()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در تغییر رمز عبور.')
    } finally {
      setPasswordSubmitting(false)
    }
  }

  const uploadButton = (
    <button
      style={{ border: 0, background: 'none', cursor: 'pointer', color: '#fff' }}
      type="button"
    >
      <PlusOutlined style={{ fontSize: 18 }} />
      <div style={{ marginTop: 8 }}>آپلود</div>
    </button>
  )

  return (
    <>
      {/* عکسِ آپلودِ دایره‌ای سمت راست، نام + شماره پرسنلی کنارش */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 8 }}>
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

        {/* بلوکِ نام + شماره پرسنلی کمی از عکس فاصله می‌گیرد (به سمت چپ/وسط). */}
        <div style={{ minWidth: 0, marginInlineStart: 24 }}>
          <div style={{ fontWeight: 700, fontSize: 20 }}>
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

      {/* خطِ جداکننده بین سربرگِ پروفایل (عکس و نام) و گزینه‌های پایین، مثل نمونه. */}
      <Divider style={{ margin: '31px 0 8px', borderColor: 'rgba(255,255,255,0.45)' }} />

      <Button
        type="text"
        icon={<KeyOutlined style={{ fontSize: 18 }} />}
        onClick={() => setPasswordOpen(true)}
        block
        style={{ textAlign: 'start', justifyContent: 'flex-start', marginBlock: 10, fontSize: 16 }}
      >
        تغییر رمز عبور
      </Button>

      {/* خطِ جداکننده بین «تغییر رمز عبور» و «تاریخچه ورودهای اخیر». */}
      <Divider style={{ margin: '8px 0', borderColor: 'rgba(255,255,255,0.45)' }} />

      <Button
        type="text"
        icon={<HistoryOutlined style={{ fontSize: 18 }} />}
        onClick={openLoginHistory}
        block
        style={{ textAlign: 'start', justifyContent: 'flex-start', marginBlock: 10, fontSize: 16 }}
      >
        تاریخچه ورودهای اخیر
      </Button>

      {/* خطِ جداکننده بین «تاریخچه ورودهای اخیر» و «نشست‌های فعال». */}
      <Divider style={{ margin: '8px 0', borderColor: 'rgba(255,255,255,0.45)' }} />

      <Button
        type="text"
        icon={<DesktopOutlined style={{ fontSize: 18 }} />}
        onClick={openSessions}
        block
        style={{ textAlign: 'start', justifyContent: 'flex-start', marginBlock: 10, fontSize: 16 }}
      >
        نشست‌های فعال
      </Button>

      {/* خطِ جداکننده بین «نشست‌های فعال» و «خروج»، دقیقاً مثل خطِ بالای آن. */}
      <Divider style={{ margin: '8px 0', borderColor: 'rgba(255,255,255,0.45)' }} />

      <Button
        type="text"
        danger
        icon={<LogoutOutlined style={{ fontSize: 18 }} />}
        onClick={logout}
        block
        style={{ textAlign: 'start', justifyContent: 'flex-start', marginTop: 10, fontSize: 16 }}
      >
        خروج
      </Button>

      <Modal
        open={passwordOpen}
        onCancel={closePassword}
        footer={null}
        centered
        title="تغییر رمز عبور"
        destroyOnHidden
      >
        {/* خطِ جداکننده بین عنوان و فیلدها، مثل خطِ پروفایل. */}
        <Divider style={{ marginTop: 0, marginBottom: 16, borderColor: 'rgba(255,255,255,0.45)' }} />
        <Form form={passwordForm} layout="vertical" onFinish={submitPassword} requiredMark={false}>
          <Form.Item
            label="رمز عبور فعلی"
            name="currentPassword"
            rules={[{ required: true, message: 'رمز عبور فعلی را وارد کنید' }]}
          >
            <Input.Password autoComplete="current-password" placeholder="رمز عبور قبلی را وارد کنید" />
          </Form.Item>

          <Form.Item
            label="رمز عبور جدید"
            name="newPassword"
            rules={[
              { required: true, message: 'رمز عبور جدید را وارد کنید' },
              { min: 6, message: 'رمز عبور جدید باید حداقل ۶ کاراکتر باشد' },
            ]}
          >
            <Input.Password autoComplete="new-password" placeholder="رمز عبور جدید را وارد کنید" />
          </Form.Item>

          <Form.Item
            label="تأیید رمز عبور جدید"
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
            <Input.Password autoComplete="new-password" placeholder="تکرار رمز عبور جدید" />
          </Form.Item>

          <div style={{ display: 'flex', gap: 8 }}>
            <Button type="primary" htmlType="submit" loading={passwordSubmitting}>
              تأیید
            </Button>
            <Button onClick={closePassword}>بستن</Button>
          </div>
        </Form>
      </Modal>

      <Modal
        open={sessionsOpen}
        onCancel={() => setSessionsOpen(false)}
        footer={null}
        width={720}
        centered
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

      <Modal
        open={historyOpen}
        onCancel={() => setHistoryOpen(false)}
        footer={null}
        width={720}
        centered
        title="تاریخچه ورودهای اخیر"
      >
        <LoginHistoryTable history={loginHistory} loading={historyLoading} />
      </Modal>
    </>
  )
}
