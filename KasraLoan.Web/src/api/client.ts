import axios from 'axios'

// همه‌ی درخواست‌ها به /api می‌روند و Vite آن‌ها را به بک‌اند پروکسی می‌کند.
export const api = axios.create({
  baseURL: '/api',
})

// توکن در sessionStorage است (نه localStorage) تا هر تبِ مرورگر نشستِ مستقلِ خودش
// را داشته باشد: باز کردنِ /login در یک تبِ جدید صفحه‌ی ورود را نشان می‌دهد و به
// خاطرِ ورودِ تبِ دیگر خودکار به داشبورد نمی‌رود. (با بستنِ تب، نشستِ آن تب پاک می‌شود.)
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// رفرش را با نمونه‌ی جدا از axios می‌زنیم تا وارد چرخه‌ی اینترسپتور نشویم. یک
// promise مشترک نگه می‌داریم تا اگر چند درخواست هم‌زمان 401 گرفتند، فقط یک بار
// رفرش شود.
let refreshing: Promise<string | null> | null = null

async function doRefresh(): Promise<string | null> {
  const refreshToken = sessionStorage.getItem('refreshToken')
  if (!refreshToken) return null
  try {
    const res = await axios.post('/api/auth/refresh', { refreshToken })
    sessionStorage.setItem('accessToken', res.data.accessToken)
    sessionStorage.setItem('refreshToken', res.data.refreshToken)
    return res.data.accessToken as string
  } catch {
    return null
  }
}

/** یک‌بار رفرش می‌کند و توکنِ جدید (یا null در صورت انقضای idle) برمی‌گرداند. */
export function refreshAccessToken(): Promise<string | null> {
  if (!refreshing) {
    refreshing = doRefresh().finally(() => {
      refreshing = null
    })
  }
  return refreshing
}

function clearAndRedirect() {
  sessionStorage.removeItem('accessToken')
  sessionStorage.removeItem('refreshToken')
  if (window.location.pathname !== '/login') {
    window.location.href = '/login'
  }
}

// روی 401 (توکنِ دسترسیِ منقضی) یک‌بار رفرش را امتحان می‌کنیم: اگر نشست هنوز زنده
// باشد توکنِ تازه می‌گیریم و همان درخواست را دوباره می‌فرستیم؛ اگر رفرش هم رد شد
// یعنی نشست به‌خاطر بی‌کاری منقضی شده، پس به صفحه‌ی ورود می‌رویم.
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config
    if (error.response?.status === 401 && original && !original._retry) {
      original._retry = true
      const newToken = await refreshAccessToken()
      if (newToken) {
        original.headers = original.headers ?? {}
        original.headers.Authorization = `Bearer ${newToken}`
        return api(original)
      }
      clearAndRedirect()
    }
    return Promise.reject(error)
  },
)
