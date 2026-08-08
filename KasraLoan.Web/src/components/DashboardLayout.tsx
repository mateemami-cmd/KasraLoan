import { type ReactNode } from 'react'
import { Layout, Menu, Typography, Button, Avatar, Space } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import type { MenuProps } from 'antd'
import { useAuth } from '../auth/AuthContext'
import { NotificationBell } from './NotificationBell'

const { Header, Sider, Content } = Layout

interface Props {
  menuItems: MenuProps['items']
  selectedKey: string
  onSelect: (key: string) => void
  children: ReactNode
  defaultOpenKeys?: string[]
  hideLogout?: boolean
  /** نوار کناری باریک (سبک نکسوس): آیکون بالا، متن پایین. */
  rail?: boolean
  /** اگر داده شود، عکس کاربر پایینِ نوارِ باریک نمایش داده می‌شود و با کلیک این تابع صدا زده می‌شود. */
  onAvatarClick?: () => void
}

export function DashboardLayout({
  menuItems,
  selectedKey,
  onSelect,
  children,
  defaultOpenKeys,
  hideLogout,
  rail,
  onAvatarClick,
}: Props) {
  const { user, logout } = useAuth()

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        theme="light"
        width={rail ? 84 : 456}
        breakpoint="lg"
        collapsedWidth={0}
        className={rail ? 'app-sider' : undefined}
      >
        <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          <div className="brand">
            <Typography.Text strong style={{ fontSize: rail ? 15 : 20 }}>
              کسرا
            </Typography.Text>
          </div>

          <Menu
            mode={rail ? 'vertical' : 'inline'}
            selectedKeys={[selectedKey]}
            defaultOpenKeys={defaultOpenKeys}
            items={
              rail
                ? menuItems
                : [
                    { key: '__dashboard_label', type: 'group', label: 'Dashboard' },
                    ...(menuItems ?? []),
                  ]
            }
            onClick={(e) => onSelect(e.key)}
            style={{ flex: 1, borderInlineEnd: 'none' }}
          />

          {rail && onAvatarClick && (
            <div
              style={{
                padding: '12px 0',
                borderTop: '1px solid #f0f0f0',
                textAlign: 'center',
              }}
            >
              <div
                onClick={onAvatarClick}
                title="پروفایل"
                style={{ cursor: 'pointer', display: 'inline-block' }}
              >
                <Avatar
                  size={44}
                  src={user?.profilePictureUrl || undefined}
                  icon={<UserOutlined />}
                />
              </div>
            </div>
          )}

          {!hideLogout &&
            (rail ? (
              <div
                style={{
                  padding: '10px 4px',
                  borderTop: '1px solid #f0f0f0',
                  textAlign: 'center',
                }}
              >
                <Button
                  type="text"
                  danger
                  onClick={logout}
                  style={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: 2,
                    height: 'auto',
                    width: '100%',
                    fontSize: 12,
                  }}
                >
                  <LogoutOutlined style={{ fontSize: 18 }} />
                  خروج
                </Button>
              </div>
            ) : (
              <div style={{ padding: 12, borderTop: '1px solid #f0f0f0' }}>
                <Button icon={<LogoutOutlined />} onClick={logout} danger block>
                  خروج
                </Button>
              </div>
            ))}
        </div>
      </Sider>

      <Layout>
        <Header className="app-header">
          <span />

          <Space size="middle">
            <NotificationBell />
            <Space>
              <Avatar icon={<UserOutlined />} />
              <span>
                {user?.firstName} {user?.lastName}
              </span>
            </Space>
          </Space>
        </Header>

        <Content className="app-content">{children}</Content>
      </Layout>
    </Layout>
  )
}
