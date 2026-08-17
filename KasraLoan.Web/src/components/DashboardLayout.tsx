import { type ReactNode } from 'react'
import { Layout, Menu, Button, Avatar } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import type { MenuProps } from 'antd'
import { useAuth } from '../auth/AuthContext'

const { Sider, Content } = Layout

interface Props {
  menuItems: MenuProps['items']
  selectedKey: string
  onSelect: (key: string) => void
  children: ReactNode
  defaultOpenKeys?: string[]
  hideLogout?: boolean
  /** نوار کناری باریک (سبک نکسوس): آیکون بالا، متن پایین. */
  rail?: boolean
  /**
   * نوار باریکِ فقط‌آیکون: متن زیر آیکون نمایش داده نمی‌شود و فقط با hover
   * به‌صورت تولتیپ کنارِ آیکون می‌آید. برای داشبورد ادمین.
   */
  collapsedRail?: boolean
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
  collapsedRail,
  onAvatarClick,
}: Props) {
  const { user, logout } = useAuth()

  // نوارِ فقط‌آیکون هم یک نوعِ rail است؛ عرض و آواتار مشترک‌اند.
  const isRail = rail || collapsedRail

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        theme="dark"
        width={isRail ? 84 : 456}
        breakpoint="lg"
        collapsedWidth={0}
        // کلاسِ app-sider متن را زیر آیکون می‌چیند؛ برای حالت فقط‌آیکون آن را
        // نمی‌گذاریم تا inlineCollapsed خودِ antd (تولتیپ کنار آیکون) کار کند.
        className={rail ? 'app-sider' : undefined}
      >
        <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          <Menu
            mode={collapsedRail ? 'inline' : rail ? 'vertical' : 'inline'}
            inlineCollapsed={collapsedRail}
            selectedKeys={[selectedKey]}
            defaultOpenKeys={defaultOpenKeys}
            items={
              isRail
                ? menuItems
                : [
                    { key: '__dashboard_label', type: 'group', label: 'Dashboard' },
                    ...(menuItems ?? []),
                  ]
            }
            onClick={(e) => onSelect(e.key)}
            style={{ flex: 1, borderInlineEnd: 'none' }}
          />

          {isRail && onAvatarClick && (
            <div
              style={{
                padding: '16px 0',
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
            (isRail ? (
              <div
                style={{
                  padding: '10px 4px',
                  borderTop: '1px solid var(--border-soft)',
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
              <div style={{ padding: 12, borderTop: '1px solid var(--border-soft)' }}>
                <Button icon={<LogoutOutlined />} onClick={logout} danger block>
                  خروج
                </Button>
              </div>
            ))}
        </div>
      </Sider>

      <Layout>
        <Content className="app-content">{children}</Content>
      </Layout>
    </Layout>
  )
}
