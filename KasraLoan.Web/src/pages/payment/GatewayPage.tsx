import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Card, Form, Input, Button, Alert, Result, Spin, Row, Col } from 'antd'
import { getGatewaySession, payViaGateway } from '../../api/services'
import type { GatewaySession } from '../../api/types'

/**
 * صفحه‌ی پرداخت نمادین.
 *
 * عمداً بیرون از داشبورد و روی مسیر جداست تا مثل یک درگاه واقعی، کاربر از
 * برنامه «خارج» شود و برگردد. وقتی درگاه واقعی اضافه شود، همین مسیر جایش را
 * به آدرس بانک می‌دهد و بقیه‌ی جریان دست نمی‌خورد.
 *
 * هیچ‌کدام از مقادیر کارت در state سراسری، localStorage یا لاگ نگه داشته
 * نمی‌شوند؛ فقط یک بار ارسال می‌شوند.
 */
export function GatewayPage() {
  const { authority = '' } = useParams()
  const navigate = useNavigate()

  const [session, setSession] = useState<GatewaySession | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [paying, setPaying] = useState(false)
  const [refId, setRefId] = useState<string | null>(null)

  useEffect(() => {
    getGatewaySession(authority)
      .then(setSession)
      .catch((err) => setError(err.response?.data?.message ?? 'نشست پرداخت معتبر نیست.'))
      .finally(() => setLoading(false))
  }, [authority])

  async function onFinish(values: {
    cardNumber: string
    cvv2: string
    expiryMonth: string
    expiryYear: string
    secondPassword: string
  }) {
    setPaying(true)
    setError(null)
    try {
      const result = await payViaGateway(authority, values)
      setRefId(result.gatewayRefId ?? '—')
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      setError(e.response?.data?.message ?? 'پرداخت ناموفق بود.')
    } finally {
      setPaying(false)
    }
  }

  const money = (v: number) => `${v.toLocaleString('fa-IR')} تومان`

  if (loading) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', minHeight: '100vh' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (refId) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', minHeight: '100vh', padding: 16 }}>
        <Card style={{ maxWidth: 520, width: '100%' }}>
          <Result
            status="success"
            title="پرداخت با موفقیت انجام شد"
            subTitle={
              <div style={{ lineHeight: 2 }}>
                <div>شماره پیگیری: <b style={{ fontFamily: 'monospace' }}>{refId}</b></div>
                {session && <div>مبلغ: {money(session.amount)}</div>}
              </div>
            }
            extra={
              <Button type="primary" onClick={() => navigate('/employee')}>
                بازگشت به سامانه
              </Button>
            }
          />
        </Card>
      </div>
    )
  }

  if (!session) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', minHeight: '100vh', padding: 16 }}>
        <Card style={{ maxWidth: 520, width: '100%' }}>
          <Result
            status="error"
            title="نشست پرداخت در دسترس نیست"
            subTitle={error}
            extra={
              <Button onClick={() => navigate('/employee')}>بازگشت به سامانه</Button>
            }
          />
        </Card>
      </div>
    )
  }

  return (
    <div style={{ display: 'grid', placeItems: 'center', minHeight: '100vh', padding: 16 }}>
      <Card
        style={{ maxWidth: 520, width: '100%' }}
        title={
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span>{session.gatewayName}</span>
            <span style={{ fontSize: 13, color: 'var(--text-muted)' }}>
              قسط شماره {session.installmentNumber.toLocaleString('fa-IR')}
            </span>
          </div>
        }
      >
        {/* بدون این هشدار، ممکن است کسی سر جلسه از روی عادت کارت واقعی‌اش را بزند. */}
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message="محیط آزمایشی — پرداخت واقعی انجام نمی‌شود"
          description="اطلاعات کارت واقعی وارد نکنید. این صفحه فقط برای نمایش جریان پرداخت است."
        />

        <div
          style={{
            textAlign: 'center',
            padding: '12px 0 20px',
            fontSize: 22,
            fontWeight: 700,
          }}
        >
          {money(session.amount)}
        </div>

        {error && (
          <Alert type="error" showIcon style={{ marginBottom: 16 }} message={error} />
        )}

        <Form layout="vertical" onFinish={onFinish} disabled={paying}>
          <Form.Item
            label="شماره کارت"
            name="cardNumber"
            rules={[{ required: true, message: 'شماره کارت را وارد کنید' }]}
          >
            <Input
              placeholder="۱۶ رقم"
              maxLength={19}
              style={{ direction: 'ltr', textAlign: 'center', letterSpacing: 2 }}
            />
          </Form.Item>

          <Row gutter={12}>
            <Col span={8}>
              <Form.Item
                label="CVV2"
                name="cvv2"
                rules={[{ required: true, message: 'CVV2 لازم است' }]}
              >
                <Input maxLength={4} style={{ direction: 'ltr', textAlign: 'center' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                label="ماه انقضا"
                name="expiryMonth"
                rules={[{ required: true, message: 'ماه' }]}
              >
                <Input placeholder="۰۸" maxLength={2} style={{ direction: 'ltr', textAlign: 'center' }} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                label="سال انقضا"
                name="expiryYear"
                rules={[{ required: true, message: 'سال' }]}
              >
                <Input placeholder="۰۷" maxLength={2} style={{ direction: 'ltr', textAlign: 'center' }} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label="رمز دوم (پویا)"
            name="secondPassword"
            rules={[{ required: true, message: 'رمز دوم را وارد کنید' }]}
            extra="دقیقاً ۶ رقم"
          >
            <Input.Password
              maxLength={6}
              style={{ direction: 'ltr', textAlign: 'center', letterSpacing: 4 }}
            />
          </Form.Item>

          <Row gutter={12}>
            <Col span={16}>
              <Button type="primary" htmlType="submit" block loading={paying}>
                پرداخت
              </Button>
            </Col>
            <Col span={8}>
              <Button block onClick={() => navigate('/employee')} disabled={paying}>
                انصراف
              </Button>
            </Col>
          </Row>
        </Form>
      </Card>
    </div>
  )
}
