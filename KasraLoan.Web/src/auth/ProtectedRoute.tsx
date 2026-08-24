import { Navigate } from 'react-router-dom'
import { Spin } from 'antd'
import { useAuth } from './AuthContext'
import { ForcedResetPassword } from './ForcedResetPassword'
import type { ReactNode } from 'react'

interface Props {
  children: ReactNode
  role?: 'Admin' | 'Employee'
}

// از مسیرهایی که نیاز به ورود دارند محافظت می‌کند و در صورت نیاز نقش را هم چک می‌کند.
export function ProtectedRoute({ children, role }: Props) {
  const { user, loading } = useAuth()

  if (loading) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', height: '100vh' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  // با رمزِ موقت وارد شده: تا رمزِ جدید نگذارد، هیچ صفحه‌ی دیگری نمی‌بیند.
  if (user.mustResetPassword) {
    return <ForcedResetPassword />
  }

  if (role && user.role !== role) {
    // اگر نقش کاربر با نقش لازم نخواند، به داشبورد خودش برمی‌گردد.
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/employee'} replace />
  }

  return <>{children}</>
}
