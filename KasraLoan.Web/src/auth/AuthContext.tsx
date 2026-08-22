import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api } from '../api/client'
import type { CurrentUser, LoginResponse, SessionInfo } from '../api/types'

// نتیجه‌ی ورود: یا موفق است، یا کاربر به سقفِ نشست‌ها رسیده و باید یکی را قطع کند.
export type LoginOutcome =
  | { status: 'ok'; user: CurrentUser }
  | { status: 'choose'; sessions: SessionInfo[] }

interface AuthContextValue {
  user: CurrentUser | null
  loading: boolean
  login: (username: string, password: string, terminateSessionId?: number) => Promise<LoginOutcome>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [loading, setLoading] = useState(true)

  // توکن در sessionStorage است (نه localStorage) تا هر تبِ مرورگر سشنِ مستقلِ
  // خودش را داشته باشد؛ این‌طوری می‌شود در یک تبِ جدید با کاربرِ دیگری وارد شد.
  useEffect(() => {
    const token = sessionStorage.getItem('accessToken')
    if (!token) {
      setLoading(false)
      return
    }
    api
      .get<CurrentUser>('/auth/me')
      .then((res) => setUser(res.data))
      .catch(() => {
        sessionStorage.removeItem('accessToken')
        sessionStorage.removeItem('refreshToken')
      })
      .finally(() => setLoading(false))
  }, [])

  async function login(
    username: string,
    password: string,
    terminateSessionId?: number,
  ): Promise<LoginOutcome> {
    const res = await api.post<LoginResponse>('/auth/login', {
      username,
      password,
      terminateSessionId,
    })

    // به سقفِ نشست‌ها رسیده: هنوز وارد نشده، باید یکی را برای قطع انتخاب کند.
    if (res.data.requiresSessionChoice) {
      return { status: 'choose', sessions: res.data.sessions ?? [] }
    }

    sessionStorage.setItem('accessToken', res.data.accessToken)
    sessionStorage.setItem('refreshToken', res.data.refreshToken)

    const me = await api.get<CurrentUser>('/auth/me')
    setUser(me.data)
    return { status: 'ok', user: me.data }
  }

  // بعد از ویرایش پروفایل، اطلاعات کاربر را دوباره از سرور می‌گیریم.
  async function refreshUser() {
    const me = await api.get<CurrentUser>('/auth/me')
    setUser(me.data)
  }

  function logout() {
    const refreshToken = sessionStorage.getItem('refreshToken')
    if (refreshToken) {
      api.post('/auth/logout', { refreshToken }).catch(() => {})
    }
    sessionStorage.removeItem('accessToken')
    sessionStorage.removeItem('refreshToken')
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
