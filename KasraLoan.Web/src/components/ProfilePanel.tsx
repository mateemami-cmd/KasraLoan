import { useEffect, useRef, useState } from 'react'
import { Button, Upload, Image, App, Modal, Tag, Divider, Form, Input } from 'antd'
import {
  LogoutOutlined,
  DesktopOutlined,
  KeyOutlined,
  HistoryOutlined,
  ArrowRightOutlined,
  CameraOutlined,
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
  UserOutlined,
  PhoneOutlined,
  MailOutlined,
  IdcardOutlined,
  SolutionOutlined,
} from '@ant-design/icons'
import { useAuth } from '../auth/AuthContext'
import {
  uploadProfilePicture,
  deleteProfilePicture,
  changePassword,
  updateProfile,
  getSessions,
  revokeSession,
  getLoginHistory,
} from '../api/services'
import { SessionsTable } from './SessionsTable'
import { LoginHistoryTable } from './LoginHistoryTable'
import type { SessionInfo, LoginHistoryItem } from '../api/types'

// مثل پروفایلِ تلگرامِ دسکتاپ: در بالای بالا یک عکسِ مربعیِ تمام‌عرض است، و با
// اسکرول به پایین به آواتارِ دایره‌ای جمع می‌شود. اندازه‌ی جمع‌شده‌ی دایره:
const AVATAR_COLLAPSED = 128

interface Props {
  /** بستنِ درِ کشویی؛ دکمه‌ی بستن روی خودِ عکس رندر می‌شود (مثل تلگرام). */
  onClose?: () => void
}

/**
 * پنل پروفایل مشترک بین داشبورد کارمند و ادمین.
 *
 * چیدمان مثل «پروفایلِ من» در تلگرامِ ویندوز است: یک عکسِ مربعیِ بزرگ که نام روی
 * آن می‌نشیند، و با اسکرول به پایین به آواتارِ دایره‌ای جمع می‌شود (و برعکس).
 * تغییر و حذفِ عکس از دکمه‌های روی همان عکس انجام می‌شود.
 */
export function ProfilePanel({ onClose }: Props) {
  const { user, logout } = useAuth()
  const { message } = App.useApp()
  const [previewOpen, setPreviewOpen] = useState(false)
  const [editing, setEditing] = useState(false)
  const [square, setSquare] = useState(true)
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
  const outerRef = useRef<HTMLDivElement>(null)
  const scrollRef = useRef<HTMLDivElement>(null)

  // عکسِ هدر بیرونِ ناحیه‌ی اسکرول است و تمام‌عرض؛ فقط اطلاعات/گزینه‌ها اسکرول
  // می‌خورند (تا اسکرول‌بار روی عکس اثر نگذارد). لِسِنرِ چرخ روی کلِ پنل است:
  // در بالای بالا، چرخ به پایین عکس را به دایره جمع می‌کند و چرخ به بالا دوباره
  // مربعش می‌کند؛ در غیر این صورت اگر چرخ روی هدر باشد، اسکرول را به ناحیه‌ی
  // محتوا هدایت می‌کنیم. non-passive تا بتوانیم در لحظه‌ی مورف جلوی اسکرول را بگیریم.
  useEffect(() => {
    const el = outerRef.current
    if (!el) return
    const onWheel = (e: WheelEvent) => {
      if (editing) return
      const sc = scrollRef.current
      const atTop = !sc || sc.scrollTop <= 0
      if (e.deltaY > 0 && square && atTop) {
        setSquare(false)
        e.preventDefault()
      } else if (e.deltaY < 0 && !square && atTop) {
        setSquare(true)
        e.preventDefault()
      } else if (sc && !sc.contains(e.target as Node)) {
        // چرخ روی هدر (بیرونِ ناحیه‌ی اسکرول) → دستی به محتوا اسکرول بده.
        sc.scrollTop += e.deltaY
        e.preventDefault()
      }
    }
    el.addEventListener('wheel', onWheel, { passive: false })
    return () => el.removeEventListener('wheel', onWheel)
  }, [square, editing])

  if (!user) return null

  // صفحه‌ی «ویرایش پروفایل» (مثل تلگرام): با زدنِ مداد باز می‌شود.
  if (editing) return <EditProfileView onBack={() => setEditing(false)} />

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

  const fullName = `${user.firstName} ${user.lastName}`.trim()
  const initials = `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.trim()
  const photoUrl = user.profilePictureUrl || ''

  // ردیف‌های اطلاعاتیِ زیرِ عکس (مثل تلگرام: مقدار بالا، برچسب پایین).
  const infoRows = [
    { icon: <UserOutlined />, label: 'نام کاربری', value: `@${user.username}`, ltr: true },
    { icon: <PhoneOutlined />, label: 'شماره تماس', value: user.phoneNumber, ltr: true },
    { icon: <MailOutlined />, label: 'ایمیل', value: user.email, ltr: true },
    { icon: <IdcardOutlined />, label: 'شماره پرسنلی', value: `#${user.personnelNumber}`, ltr: true },
    { icon: <SolutionOutlined />, label: 'سمت', value: user.jobPositionTitle, ltr: false },
  ].filter((r) => r.value)

  const circleBtn = {
    background: 'rgba(0,0,0,0.35)',
    color: '#fff',
    border: 'none',
    backdropFilter: 'blur(4px)',
  } as const

  return (
    <div
      ref={outerRef}
      style={{
        height: '100%',
        overflow: 'hidden',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      {/* ---------- سربرگ: عکسِ مربعیِ تمام‌عرض که با اسکرول به پایین به آواتارِ دایره‌ای جمع می‌شود ---------- */}
      <div
        style={{
          position: 'relative',
          flex: '0 0 auto',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          paddingTop: square ? 0 : 24,
          paddingBottom: square ? 0 : 12,
        }}
      >
        {/* بلوکِ مورف‌شونده: مربعِ تمام‌عرض ↔ دایره. با aspect-ratio ارتفاع همیشه
            برابرِ عرض می‌ماند (مربعِ دقیق)، و فقط عرض و گوشه انیمیت می‌شوند تا روان باشد. */}
        <div
          onClick={() => (square ? photoUrl && setPreviewOpen(true) : setSquare(true))}
          style={{
            position: 'relative',
            overflow: 'hidden',
            flex: '0 0 auto',
            width: square ? '100%' : AVATAR_COLLAPSED,
            aspectRatio: '1 / 1',
            borderRadius: square ? 0 : '50%',
            willChange: 'width, border-radius',
            transition:
              'width 0.34s cubic-bezier(0.4, 0, 0.2, 1), border-radius 0.34s cubic-bezier(0.4, 0, 0.2, 1)',
            background: photoUrl
              ? '#000'
              : 'linear-gradient(135deg, #1677ff 0%, #4096ff 60%, #69b1ff 100%)',
            cursor: 'pointer',
          }}
        >
          {photoUrl ? (
            <img
              src={photoUrl}
              alt={fullName}
              style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
            />
          ) : (
            <div
              style={{
                width: '100%',
                height: '100%',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#fff',
                fontSize: square ? 88 : 46,
                fontWeight: 700,
              }}
            >
              {initials || <UserOutlined />}
            </div>
          )}

          {/* نام روی عکس فقط در حالتِ بازشده (روی عکسِ تمام‌عرض). */}
          {square && (
            <div
              style={{
                position: 'absolute',
                insetInline: 0,
                bottom: 0,
                padding: '48px 20px 16px',
                background: 'linear-gradient(to top, rgba(0,0,0,0.72), rgba(0,0,0,0))',
                color: '#fff',
                pointerEvents: 'none',
              }}
            >
              <div style={{ fontSize: 22, fontWeight: 700, lineHeight: 1.3 }}>{fullName}</div>
            </div>
          )}
        </div>

        {/* نام زیرِ آواتار فقط در حالتِ جمع‌شده (دایره‌ای). */}
        {!square && (
          <div style={{ textAlign: 'center', marginTop: 12 }}>
            <div style={{ fontSize: 20, fontWeight: 700, lineHeight: 1.3 }}>{fullName}</div>
          </div>
        )}

        {/* نوارِ بالای پنل: بستن (سمتِ راست) + ویرایشِ پروفایل (مداد، سمتِ چپ). */}
        <div
          style={{
            position: 'absolute',
            top: 12,
            insetInline: 12,
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <Button
            type="text"
            shape="circle"
            style={circleBtn}
            icon={<ArrowRightOutlined />}
            onClick={onClose}
          />
          <Button
            type="text"
            shape="circle"
            style={circleBtn}
            icon={<EditOutlined />}
            onClick={() => setEditing(true)}
          />
        </div>
      </div>

      {/* ---------- ناحیه‌ی اسکرول: فقط اطلاعات و گزینه‌ها اسکرول می‌خورند (نوارِ اسکرول پنهان) ---------- */}
      <div
        ref={scrollRef}
        className="profile-scroll"
        style={{ flex: '1 1 auto', minHeight: 0, overflowY: 'auto' }}
      >
      {/* ---------- ردیف‌های اطلاعاتی ---------- */}
      <div style={{ padding: '8px 20px' }}>
        {infoRows.map((row) => (
          <div
            key={row.label}
            style={{ display: 'flex', alignItems: 'center', gap: 14, padding: '10px 0' }}
          >
            <span style={{ color: 'var(--text-muted)', fontSize: 18, flex: '0 0 auto' }}>
              {row.icon}
            </span>
            <div style={{ minWidth: 0 }}>
              <div
                style={{
                  fontSize: 15,
                  direction: row.ltr ? 'ltr' : 'rtl',
                  textAlign: 'start',
                  wordBreak: 'break-all',
                }}
              >
                {row.value}
              </div>
              <div style={{ color: 'var(--text-muted)', fontSize: 12 }}>{row.label}</div>
            </div>
          </div>
        ))}
      </div>

      <Divider style={{ margin: '4px 0', borderColor: 'rgba(255,255,255,0.15)' }} />

      {/* ---------- گزینه‌ها (خروج جدا و در کفِ پنل است) ---------- */}
      <div style={{ padding: '4px 12px 16px' }}>
        <Button
          type="text"
          icon={<KeyOutlined style={{ fontSize: 18 }} />}
          onClick={() => setPasswordOpen(true)}
          block
          style={{ textAlign: 'start', justifyContent: 'flex-start', marginBlock: 14, fontSize: 16 }}
        >
          تغییر رمز عبور
        </Button>

        <Button
          type="text"
          icon={<HistoryOutlined style={{ fontSize: 18 }} />}
          onClick={openLoginHistory}
          block
          style={{ textAlign: 'start', justifyContent: 'flex-start', marginBlock: 14, fontSize: 16 }}
        >
          تاریخچه ورودهای اخیر
        </Button>

        <Button
          type="text"
          icon={<DesktopOutlined style={{ fontSize: 18 }} />}
          onClick={openSessions}
          block
          style={{ textAlign: 'start', justifyContent: 'flex-start', marginBlock: 14, fontSize: 16 }}
        >
          نشست‌های فعال
        </Button>
      </div>
      </div>

      {/* ---------- کفِ پنل: خروج همیشه پایین می‌ماند ---------- */}
      <div
        style={{
          flex: '0 0 auto',
          padding: '8px 12px',
          borderTop: '1px solid rgba(255,255,255,0.15)',
        }}
      >
        <Button
          type="text"
          danger
          icon={<LogoutOutlined style={{ fontSize: 18 }} />}
          onClick={logout}
          block
          style={{ textAlign: 'start', justifyContent: 'flex-start', fontSize: 16 }}
        >
          خروج
        </Button>
      </div>

      {photoUrl && (
        <Image
          styles={{ root: { display: 'none' } }}
          preview={{
            open: previewOpen,
            onOpenChange: (visible) => setPreviewOpen(visible),
          }}
          src={photoUrl}
        />
      )}

      <Modal
        open={passwordOpen}
        onCancel={closePassword}
        footer={null}
        centered
        title="تغییر رمز عبور"
        destroyOnHidden
      >
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
    </div>
  )
}

/**
 * صفحه‌ی «ویرایش پروفایل» (سبک تلگرام): تغییر/حذفِ عکس + ویرایشِ شماره تماس و
 * ایمیل. نام/نام‌خانوادگی و نام‌کاربری توسط مدیر تنظیم می‌شوند و اینجا فقط‌خواندنی‌اند.
 */
function EditProfileView({ onBack }: { onBack: () => void }) {
  const { user, refreshUser } = useAuth()
  const { message, modal } = App.useApp()
  const [saving, setSaving] = useState(false)
  const [form] = Form.useForm()

  if (!user) return null

  const fullName = `${user.firstName} ${user.lastName}`.trim()
  const initials = `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.trim()
  const photoUrl = user.profilePictureUrl || ''
  const employmentLabel = user.employmentStatus === 'Active' ? 'فعال' : 'خاتمه‌یافته'

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

  function confirmRemovePhoto() {
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
      },
    })
  }

  async function onFinish(values: {
    additionalPhoneNumbers?: string[]
    additionalEmails?: string[]
  }) {
    setSaving(true)
    try {
      // شماره و ایمیلِ اصلی را ادمین تعیین می‌کند و اینجا ارسال نمی‌شوند؛ فقط
      // شماره و ایمیلِ اضافه (هرکدام حداکثر یکی) به‌روزرسانی می‌شوند.
      await updateProfile({
        additionalPhoneNumbers: (values.additionalPhoneNumbers ?? [])
          .map((p) => p?.trim())
          .filter((p): p is string => !!p),
        additionalEmails: (values.additionalEmails ?? [])
          .map((e) => e?.trim())
          .filter((e): e is string => !!e),
      })
      await refreshUser()
      message.success('پروفایل به‌روزرسانی شد.')
      onBack()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ذخیره‌ی پروفایل.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
      {/* هدر: بازگشت + عنوان + ذخیره */}
      <div
        style={{
          flex: '0 0 auto',
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          padding: 12,
          borderBottom: '1px solid rgba(255,255,255,0.12)',
        }}
      >
        <Button type="text" shape="circle" icon={<ArrowRightOutlined />} onClick={onBack} />
        <span style={{ fontSize: 18, fontWeight: 700, flex: 1 }}>ویرایش پروفایل</span>
        <Button type="primary" loading={saving} onClick={() => form.submit()}>
          ذخیره
        </Button>
      </div>

      {/* بدنه‌ی اسکرول‌شونده */}
      <div
        className="profile-scroll"
        style={{ flex: '1 1 auto', minHeight: 0, overflowY: 'auto', padding: 20 }}
      >
        {/* عکس با دکمه‌ی دوربین برای تغییر */}
        <div
          style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 10,
            marginBottom: 20,
          }}
        >
          <div style={{ position: 'relative', width: 112, height: 112 }}>
            <div
              style={{
                width: 112,
                height: 112,
                borderRadius: '50%',
                overflow: 'hidden',
                background: photoUrl ? '#000' : 'linear-gradient(135deg, #1677ff, #69b1ff)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#fff',
                fontSize: 40,
                fontWeight: 700,
              }}
            >
              {photoUrl ? (
                <img
                  src={photoUrl}
                  alt={fullName}
                  style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                />
              ) : (
                initials || <UserOutlined />
              )}
            </div>
            <Upload
              showUploadList={false}
              accept="image/png,image/jpeg,image/webp"
              beforeUpload={beforeUpload}
            >
              <Button
                type="primary"
                shape="circle"
                icon={<CameraOutlined />}
                style={{ position: 'absolute', insetInlineStart: 0, bottom: 0 }}
              />
            </Upload>
          </div>
          {photoUrl && (
            <Button
              type="text"
              danger
              size="small"
              icon={<DeleteOutlined />}
              onClick={confirmRemovePhoto}
            >
              حذف عکس
            </Button>
          )}
        </div>

        <Form
          form={form}
          layout="vertical"
          requiredMark={false}
          onFinish={onFinish}
          initialValues={{
            additionalPhoneNumbers: user.additionalPhoneNumbers?.slice(0, 1) ?? [],
            additionalEmails: user.additionalEmails?.slice(0, 1) ?? [],
          }}
        >
          {/* ——— اطلاعاتِ شناسایی و شغلی: فقط‌خواندنی (فقط مدیر تغییرشان می‌دهد) ——— */}
          <Divider orientation="right" style={{ margin: '0 0 12px', borderColor: 'rgba(255,255,255,0.15)' }}>
            اطلاعات شخصی
          </Divider>

          <Form.Item label="نام و نام خانوادگی">
            <Input value={fullName} disabled />
          </Form.Item>

          <Form.Item label="نام کاربری">
            <Input value={user.username} disabled prefix="@" />
          </Form.Item>

          <Form.Item label="شماره پرسنلی">
            <Input value={user.personnelNumber} disabled prefix="#" />
          </Form.Item>

          {user.jobPositionTitle && (
            <Form.Item label="سمت">
              <Input value={user.jobPositionTitle} disabled />
            </Form.Item>
          )}

          <Form.Item label="وضعیت اشتغال">
            <Input value={employmentLabel} disabled />
          </Form.Item>

          {/* ——— اطلاعاتِ تماس ——— */}
          <Divider orientation="right" style={{ margin: '4px 0 12px', borderColor: 'rgba(255,255,255,0.15)' }}>
            اطلاعات تماس
          </Divider>

          {/* شماره‌ی اصلی: ادمین تعیین می‌کند و اینجا فقط‌خواندنی است. */}
          <Form.Item label="شماره تماس (اصلی)">
            <Input
              value={user.phoneNumber || 'ثبت‌نشده'}
              disabled
              style={{ direction: 'ltr', textAlign: 'right' }}
            />
          </Form.Item>

          {/* یک شماره‌ی دوم که کاربر می‌تواند اضافه/حذف/جایگزین کند (حداکثر یکی). */}
          <Form.List name="additionalPhoneNumbers">
            {(fields, { add, remove }) => (
              <Form.Item label="شماره تماس دوم">
                {fields.map((field) => (
                  <div key={field.key} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                    <Form.Item
                      name={field.name}
                      noStyle
                      rules={[
                        { pattern: /^09\d{9}$/, message: 'شماره باید موبایل معتبر باشد (۰۹...)' },
                      ]}
                    >
                      <Input
                        placeholder="مثلاً 09123456789"
                        inputMode="numeric"
                        style={{ direction: 'ltr', textAlign: 'right' }}
                      />
                    </Form.Item>
                    <Button
                      type="text"
                      danger
                      icon={<DeleteOutlined />}
                      onClick={() => remove(field.name)}
                    />
                  </div>
                ))}
                {fields.length < 1 && (
                  <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>
                    افزودن شماره دوم
                  </Button>
                )}
              </Form.Item>
            )}
          </Form.List>

          {/* ایمیلِ اصلی: ادمین تعیین می‌کند و اینجا فقط‌خواندنی است. */}
          <Form.Item label="ایمیل (اصلی)">
            <Input
              value={user.email || 'ثبت‌نشده'}
              disabled
              style={{ direction: 'ltr', textAlign: 'right' }}
            />
          </Form.Item>

          {/* یک ایمیلِ دوم که کاربر می‌تواند اضافه/حذف/جایگزین کند (حداکثر یکی). */}
          <Form.List name="additionalEmails">
            {(fields, { add, remove }) => (
              <Form.Item label="ایمیل دوم">
                {fields.map((field) => (
                  <div key={field.key} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                    <Form.Item
                      name={field.name}
                      noStyle
                      rules={[{ type: 'email', message: 'ایمیل معتبر نیست' }]}
                    >
                      <Input
                        placeholder="name@example.com"
                        style={{ direction: 'ltr', textAlign: 'right' }}
                      />
                    </Form.Item>
                    <Button
                      type="text"
                      danger
                      icon={<DeleteOutlined />}
                      onClick={() => remove(field.name)}
                    />
                  </div>
                ))}
                {fields.length < 1 && (
                  <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>
                    افزودن ایمیل دوم
                  </Button>
                )}
              </Form.Item>
            )}
          </Form.List>
        </Form>
      </div>
    </div>
  )
}
