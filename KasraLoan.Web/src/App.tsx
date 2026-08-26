import { Routes, Route, Navigate } from 'react-router-dom'
import { LoginPage } from './pages/LoginPage'
import { ForgotPage } from './pages/ForgotPage'
import { EmployeeDashboard } from './pages/employee/EmployeeDashboard'
import { AdminDashboard } from './pages/admin/AdminDashboard'
import { GatewayPage } from './pages/payment/GatewayPage'
import { ProtectedRoute } from './auth/ProtectedRoute'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/Forgot" element={<ForgotPage />} />

      <Route
        path="/employee/*"
        element={
          <ProtectedRoute role="Employee">
            <EmployeeDashboard />
          </ProtectedRoute>
        }
      />

      <Route
        path="/admin/*"
        element={
          <ProtectedRoute role="Admin">
            <AdminDashboard />
          </ProtectedRoute>
        }
      />

      {/* صفحه‌ی درگاه بیرون از داشبورد است تا مثل یک درگاه واقعی رفتار کند. */}
      <Route
        path="/payment/gateway/:authority"
        element={
          <ProtectedRoute role="Employee">
            <GatewayPage />
          </ProtectedRoute>
        }
      />

      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  )
}
