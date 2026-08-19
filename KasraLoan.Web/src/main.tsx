import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ConfigProvider, App as AntApp, theme } from 'antd'
import faIR from 'antd/locale/fa_IR'
import dayjs from 'dayjs'
import jalaliday from 'jalaliday/dayjs'
import updateLocale from 'dayjs/plugin/updateLocale'
import 'dayjs/locale/fa'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './auth/AuthContext.tsx'

// تقویمِ همه‌ی دیت‌پیکرها شمسی شود. کاربر ایرانی است و باید تاریخ را شمسی
// ببیند و انتخاب کند. مقدارِ زیرین همان لحظه‌ی مطلق است، پس toISOString هنوز
// تاریخ میلادیِ درست را به بک‌اند می‌فرستد و سمت سرور چیزی عوض نمی‌شود.
dayjs.extend(jalaliday)
dayjs.extend(updateLocale)

// نام ماه‌های لوکیل fa به‌صورت پیش‌فرض میلادی است (ژانویه، …، مه، …)؛ چون AntD
// نام ماه را با شماره‌ی ماهِ شمسی از همین آرایه برمی‌دارد، باید نام‌ها را شمسی کنیم
// وگرنه مثلاً مرداد را «مه» نشان می‌دهد. کل برنامه شمسی است، پس این جایگزینی بی‌خطر است.
const jalaliMonths =
  'فروردین_اردیبهشت_خرداد_تیر_مرداد_شهریور_مهر_آبان_آذر_دی_بهمن_اسفند'.split('_')
dayjs.updateLocale('fa', {
  months: jalaliMonths,
  monthsShort: jalaliMonths,
})

dayjs.locale('fa')
dayjs.calendar('jalali')

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider
      direction="rtl"
      locale={faIR}
      theme={{
        algorithm: theme.darkAlgorithm,
        token: {
          colorPrimary: '#2f80ff',
          colorInfo: '#2f80ff',
          colorBgLayout: '#0f1a33',
          colorBgContainer: '#182640',
          colorBgElevated: '#1e2c4a',
          fontFamily: 'Vazirmatn, Tahoma, sans-serif',
          borderRadius: 10,
        },
      }}
    >
      <AntApp>
        <BrowserRouter>
          <AuthProvider>
            <App />
          </AuthProvider>
        </BrowserRouter>
      </AntApp>
    </ConfigProvider>
  </StrictMode>,
)
