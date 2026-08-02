import { useEffect, useState } from 'react'
import { Badge, Popover, List, Button, Empty, Typography } from 'antd'
import { BellOutlined } from '@ant-design/icons'
import {
  getMyNotifications,
  markAllNotificationsRead,
} from '../api/services'
import type { NotificationItem } from '../api/types'

export function NotificationBell() {
  const [items, setItems] = useState<NotificationItem[]>([])
  const [unread, setUnread] = useState(0)
  const [open, setOpen] = useState(false)

  async function load() {
    try {
      const data = await getMyNotifications()
      setItems(data.items)
      setUnread(data.unreadCount)
    } catch {
      /* silent */
    }
  }

  useEffect(() => {
    load()
    // هر ۳۰ ثانیه تعداد اعلان‌ها به‌روز می‌شود.
    const timer = setInterval(load, 30000)
    return () => clearInterval(timer)
  }, [])

  async function handleOpen(next: boolean) {
    setOpen(next)
    if (next && unread > 0) {
      await markAllNotificationsRead()
      setUnread(0)
      setItems((prev) => prev.map((n) => ({ ...n, isRead: true })))
    }
  }

  const content = (
    <div style={{ width: 320, maxHeight: 400, overflow: 'auto' }}>
      {items.length === 0 ? (
        <Empty description="اعلانی نداری" image={Empty.PRESENTED_IMAGE_SIMPLE} />
      ) : (
        <List
          dataSource={items}
          renderItem={(n) => (
            <List.Item>
              <List.Item.Meta
                title={<Typography.Text strong>{n.title}</Typography.Text>}
                description={n.message}
              />
            </List.Item>
          )}
        />
      )}
    </div>
  )

  return (
    <Popover
      content={content}
      title="اعلان‌ها"
      trigger="click"
      open={open}
      onOpenChange={handleOpen}
      placement="bottomLeft"
    >
      <Badge count={unread} size="small">
        <Button type="text" icon={<BellOutlined style={{ fontSize: 20 }} />} />
      </Badge>
    </Popover>
  )
}
