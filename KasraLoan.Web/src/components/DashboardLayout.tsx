import { type ReactNode } from 'react'
import { Layout, Menu, Typography, Button, Avatar, Space } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import type { MenuProps } from 'antd'
import { useAuth } from '../auth/AuthContext'
import { NotificationBell } from './NotificationBell'

const { Header, Sider, Content } = Layout

interface Props {
  title: string
  menuItems: MenuProps['items']
  selectedKey: string
  onSelect: (key: string) => void
  children: ReactNode
}

export function DashboardLayout({
  title,
  menuItems,
  selectedKey,
  onSelect,
  children,
}: Props) {
  const { user, logout } = useAuth()

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider theme="light" width={240} breakpoint="lg" collapsedWidth={0}>
        <div className="brand">
          <Typography.Text strong style={{ fontSize: 18 }}>
            کسری
          </Typography.Text>
        </div>
        <Menu
          mode="inline"
          selectedKeys={[selectedKey]}
          items={menuItems}
          onClick={(e) => onSelect(e.key)}
          style={{ borderInlineEnd: 'none' }}
        />
      </Sider>

      <Layout>
        <Header className="app-header">
          <Typography.Title level={4} style={{ margin: 0 }}>
            {title}
          </Typography.Title>

          <Space size="middle">
            <NotificationBell />
            <Space>
              <Avatar icon={<UserOutlined />} />
              <span>
                {user?.firstName} {user?.lastName}
              </span>
            </Space>
            <Button icon={<LogoutOutlined />} onClick={logout} danger>
              خروج
            </Button>
          </Space>
        </Header>

        <Content className="app-content">{children}</Content>
      </Layout>
    </Layout>
  )
}
