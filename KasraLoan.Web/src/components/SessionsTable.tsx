import { Table } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { ReactNode } from 'react'
import type { SessionInfo } from '../api/types'

// تاریخِ کاملِ شمسی با روزِ هفته + ساعت با ثانیه، مثل «یکشنبه ۲۴ خرداد ۱۴۰۴ ۱۳:۱۲:۲۰».
// چون ترتیبِ پیش‌فرضِ Intl برای fa-IR (سال-ماه-روز) دلخواه نیست، بخش‌ها را جدا
// می‌گیریم و به ترتیبِ «روزِ هفته روز ماه سال» می‌چینیم.
// سشن‌های قبل از این قابلیت تاریخِ نامعتبر دارند → «—».
export function formatDateTime(value: string): string {
  const d = new Date(value)
  if (isNaN(d.getTime()) || d.getFullYear() < 1900) return '—'
  const fa = (opt: Intl.DateTimeFormatOptions) => d.toLocaleDateString('fa-IR', opt)
  const weekday = fa({ weekday: 'long' })
  const day = fa({ day: 'numeric' })
  const month = fa({ month: 'long' })
  const year = fa({ year: 'numeric' })
  const time = d.toLocaleTimeString('fa-IR', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
  return `${weekday} ${day} ${month} ${year} ${time}`
}

/**
 * جدولِ مشترکِ «نشست‌های فعال» با ستون‌های آخرین دسترسی / آدرس / دستگاه / سیستم‌عامل.
 * ستونِ «عملیات» را هر جا خودش می‌سازد (خروج، یا تگِ «جاری»، یا «قطع و ورود»).
 */
export function SessionsTable({
  sessions,
  renderAction,
  loading,
}: {
  sessions: SessionInfo[]
  renderAction: (s: SessionInfo) => ReactNode
  loading?: boolean
}) {
  const columns: ColumnsType<SessionInfo> = [
    { title: 'آخرین دسترسی', dataIndex: 'lastSeenAt', render: formatDateTime },
    { title: 'آدرس', dataIndex: 'ipAddress', render: (v?: string) => v || '—' },
    { title: 'دستگاه', dataIndex: 'deviceBrowser', render: (v?: string) => v || 'نامشخص' },
    { title: 'سیستم‌عامل', dataIndex: 'deviceOs', render: (v?: string) => v || 'نامشخص' },
    { title: 'عملیات', render: (_, s) => renderAction(s) },
  ]

  return (
    <Table
      rowKey="id"
      columns={columns}
      dataSource={sessions}
      loading={loading}
      pagination={false}
      size="small"
      scroll={{ x: 'max-content' }}
    />
  )
}
