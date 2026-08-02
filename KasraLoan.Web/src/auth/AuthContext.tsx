import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api } from '../api/client'
import type { CurrentUser, LoginResponse } from '../api/types'

interface AuthContextValue {
  user: CurrentUser | null
  loading: boolean
  login: (username: string, password: string) => Promise<CurrentUser>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [loading, setLoading] = useState(true)

  // در شروع برنامه، اگر توکن ذخیره شده باشد، اطلاعات کاربر را می‌گیریم.
  useEffect(() => {
    const token = localStorage.getItem('accessToken')
    if (!token) {
      setLoading(false)
      return
    }
    api
      .get<CurrentUser>('/auth/me')
      .then((res) => setUser(res.data))
      .catch(() => {
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
      })
      .finally(() => setLoading(false))
  }, [])

  async function login(username: string, password: string): Promise<CurrentUser> {
    const res = await api.post<LoginResponse>('/auth/login', { username, password })
    localStorage.setItem('accessToken', res.data.accessToken)
    localStorage.setItem('refreshToken', res.data.refreshToken)

    const me = await api.get<CurrentUser>('/auth/me')
    setUser(me.data)
    return me.data
  }

  // بعد از ویرایش پروفایل، اطلاعات کاربر را دوباره از سرور می‌گیریم.
  async function refreshUser() {
    const me = await api.get<CurrentUser>('/auth/me')
    setUser(me.data)
  }

  function logout() {
    const refreshToken = localStorage.getItem('refreshToken')
    if (refreshToken) {
      api.post('/auth/logout', { refreshToken }).catch(() => {})
    }
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
