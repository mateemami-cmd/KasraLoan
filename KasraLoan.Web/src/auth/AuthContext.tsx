import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, refreshAccessToken } from '../api/client'
import { getDeviceId } from '../api/device'
import { registerVisit } from '../api/services'
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

  // توکن در localStorage است (ماندگار) تا با بستن و باز کردنِ تب — تا وقتی نشست
  // منقضی نشده — لازم نباشد دوباره وارد شد. اگر توکن منقضی باشد، اینترسپتورِ client
  // خودش یک‌بار رفرش را امتحان می‌کند و در صورت شکست به صفحه‌ی ورود می‌برد.
  useEffect(() => {
    const token = localStorage.getItem('accessToken')
    if (!token) {
      setLoading(false)
      return
    }
    api
      .get<CurrentUser>('/auth/me')
      .then(async (res) => {
        setUser(res.data)
        // اگر این تب تازه باز شده و ورودش هنوز ثبت نشده (auto-resume با توکنِ مشترک)،
        // یک ورود/نشستِ جدید ثبت می‌کنیم تا باز کردنِ تبِ جدید هم مثلِ یک ورودِ واقعی
        // حساب شود. sessionStorage مخصوصِ همین تب است، پس رفرشِ همین تب دوباره ثبت
        // نمی‌کند؛ ولی هر تبِ تازه یک بار ثبت می‌کند. (مارکر را قبل از درخواست ست می‌کنیم
        // تا در StrictModeِ توسعه دوبار ثبت نشود.)
        if (!sessionStorage.getItem('visitRegistered')) {
          sessionStorage.setItem('visitRegistered', '1')
          try {
            const v = await registerVisit()
            localStorage.setItem('accessToken', v.accessToken)
            localStorage.setItem('refreshToken', v.refreshToken)
          } catch {
            /* مهم نیست؛ کاربر همچنان با توکنِ فعلی وارد است. */
          }
        }
      })
      .catch(() => {
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
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

    localStorage.setItem('accessToken', res.data.accessToken)
    localStorage.setItem('refreshToken', res.data.refreshToken)
    // ورودِ با رمز خودش یک ورود است؛ مارکر را ست می‌کنیم تا رفرشِ همین تب دوباره
    // به‌عنوان «ورودِ تبِ تازه» ثبت نشود.
    sessionStorage.setItem('visitRegistered', '1')

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
    const refreshToken = localStorage.getItem('refreshToken')
    if (refreshToken) {
      api.post('/auth/logout', { refreshToken }).catch(() => {})
    }
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    sessionStorage.removeItem('visitRegistered')
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
