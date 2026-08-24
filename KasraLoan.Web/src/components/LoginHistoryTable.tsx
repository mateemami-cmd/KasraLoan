import { Table, Tag } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import type { LoginHistoryItem } from '../api/types'
import { formatDateTime } from './SessionsTable'

/**
 * جدولِ «تاریخچه ورودهای اخیر»: ستون‌هایش دقیقاً مثل جدولِ نشست‌های فعال است
 * (آخرین دسترسی / آدرس / دستگاه / سیستم‌عامل) به‌علاوه‌ی ستونِ «نتیجه» (موفق/ناموفق).
 */
export function LoginHistoryTable({
  history,
  loading,
}: {
  history: LoginHistoryItem[]
  loading?: boolean
}) {
  const columns: ColumnsType<LoginHistoryItem> = [
    { title: 'آخرین دسترسی', dataIndex: 'attemptedAt', render: formatDateTime },
    { title: 'آدرس', dataIndex: 'ipAddress', render: (v?: string) => v || '—' },
    { title: 'دستگاه', dataIndex: 'deviceBrowser', render: (v?: string) => v || 'نامشخص' },
    { title: 'سیستم‌عامل', dataIndex: 'deviceOs', render: (v?: string) => v || 'نامشخص' },
    {
      title: 'نتیجه',
      dataIndex: 'isSuccess',
      render: (v: boolean) =>
        v ? <Tag color="green">موفق</Tag> : <Tag color="red">ناموفق</Tag>,
    },
  ]

  return (
    <Table
      rowKey="attemptedAt"
      columns={columns}
      dataSource={history}
      loading={loading}
      pagination={false}
      size="small"
      scroll={{ x: 'max-content' }}
    />
  )
}
