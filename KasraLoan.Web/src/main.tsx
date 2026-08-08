import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { ConfigProvider, App as AntApp, theme } from 'antd'
import faIR from 'antd/locale/fa_IR'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './auth/AuthContext.tsx'

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
