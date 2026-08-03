import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// در حالت توسعه، هر درخواستی به /api به بک‌اند .NET روی پورت 5068 فوروارد می‌شود.
// این‌طوری نه درگیر CORS می‌شویم و نه پورت بک‌اند در کد فرانت hardcode می‌شود.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5068',
        changeOrigin: true,
      },
      // عکس‌های آپلودشده (پروفایل، مدارک) هم از بک‌اند سرو می‌شوند.
      '/uploads': {
        target: 'http://localhost:5068',
        changeOrigin: true,
      },
    },
  },
})
