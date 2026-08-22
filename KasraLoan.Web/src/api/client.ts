import axios from 'axios'

// همه‌ی درخواست‌ها به /api می‌روند و Vite آن‌ها را به بک‌اند پروکسی می‌کند.
export const api = axios.create({
  baseURL: '/api',
})

// قبل از هر درخواست، اگر توکن ذخیره شده باشد به هدر Authorization اضافه می‌شود.
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// اگر پاسخ 401 (توکن نامعتبر/منقضی) بود، کاربر را به صفحه‌ی ورود برمی‌گردانیم.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      sessionStorage.removeItem('accessToken')
      sessionStorage.removeItem('refreshToken')
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)
