import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, refreshAccessToken } from '../api/client'
import { getDeviceId } from '../api/device'
import type { CurrentUser, LoginResponse, SessionInfo } from '../api/types'

// هر چند دقیقه یک‌بار توکن را پشتِ‌صحنه تازه می‌کنیم تا مهلتِ بی‌کاریِ نشست جلو
// برود. تا وقتی تب باز است نشست زنده می‌ماند؛ با بسته‌شدنِ تب این ضربان قطع می‌شود
// و بعد از مهلتِ idle نشست منقضی می‌گردد. باید کمتر از مهلتِ idle (۱۰ دقیقه) باشد.
const HEARTBEAT_MS = 4 * 60 * 1000

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

  // توکن در sessionStorage است (مخصوصِ همین تب)، نه localStorage. پس هر تب نشستِ
  // مستقلِ خودش را دارد: باز کردنِ /login در یک تبِ جدید صفحه‌ی ورود را نشان می‌دهد و
  // به‌خاطر ورودِ تبِ دیگر خودکار به داشبورد نمی‌رود. رفرشِ همین تب توکن را نگه می‌دارد
  // (sessionStorage با refresh پاک نمی‌شود)، ولی با بستنِ تب نشست آن تب می‌رود.
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

  // ضربانِ نگه‌دارنده‌ی نشست: تا وقتی کاربر وارد است و تب باز است، هر چند دقیقه
  // توکن را تازه می‌کنیم تا مهلتِ بی‌کاری ریست شود. اگر رفرش شکست خورد یعنی نشست
  // منقضی شده؛ اینترسپتور کار را تمام می‌کند و کاربر به ورود می‌رود.
  useEffect(() => {
    if (!user) return
    const id = setInterval(() => {
      refreshAccessToken()
    }, HEARTBEAT_MS)
    return () => clearInterval(id)
  }, [user])

  async function login(
    username: string,
    password: string,
    terminateSessionId?: number,
  ): Promise<LoginOutcome> {
    const res = await api.post<LoginResponse>('/auth/login', {
      username,
      password,
      terminateSessionId,
      deviceId: getDeviceId(),
    })

    // به سقفِ نشست‌ها رسیده: هنوز وارد نشده، باید یکی را برای قطع انتخاب کند.
    // (با برداشتنِ سقف، دیگر عملاً رخ نمی‌دهد؛ برای سازگاری نگه داشته شده.)
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
