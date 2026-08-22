import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// در حالت توسعه، هر درخواستی به /api به بک‌اند .NET روی پورت 5068 فوروارد می‌شود.
// این‌طوری نه درگیر CORS می‌شویم و نه پورت بک‌اند در کد فرانت hardcode می‌شود.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // روی همه‌ی اینترفیس‌ها گوش می‌دهد تا از دستگاه‌های دیگرِ شبکه (مثلاً موبایل)
    // هم بشود وارد شد؛ آن‌وقت IP واقعیِ دستگاه در نشست‌های فعال دیده می‌شود.
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5068',
        changeOrigin: true,
        // xfwd: هدر X-Forwarded-For را با IP واقعیِ کاربر به بک‌اند می‌فرستد.
        xfwd: true,
      },
      // عکس‌های آپلودشده (پروفایل، مدارک) هم از بک‌اند سرو می‌شوند.
      '/uploads': {
        target: 'http://localhost:5068',
        changeOrigin: true,
        xfwd: true,
      },
    },
  },
})
