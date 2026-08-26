import { useEffect, useState } from 'react'
import {
  Modal,
  Form,
  Select,
  Input,
  DatePicker,
  Button,
  Alert,
  Row,
  Col,
  Card,
  Upload,
  Tag,
  Divider,
  Spin,
  App,
} from 'antd'
import { UploadOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import { getLoanQuote, createLoanRequest } from '../../api/services'
import type { LoanType, LoanQuote } from '../../api/types'

const MAX_FILES = 2

/**
 * فرم درخواست وام.
 *
 * تمام اعداد — سقف، گزینه‌های مبلغ، و مبلغ قسط برای هر تعداد قسط — از اندپوینت
 * quote می‌آیند و اینجا هیچ فرمولی تکرار نشده. اگر روزی کارمزد یا سقف عوض شود،
 * فرم خودبه‌خود درست می‌ماند.
 *
 * هر نوع وام بخش اختصاصی خودش را نشان می‌دهد (سفر، ازدواج)؛ بقیه‌ی انواع
 * فعلاً فقط مبلغ و اقساط دارند تا فرم اختصاصی‌شان نوشته شود.
 */
export function LoanRequestModal({
  loanType,
  onClose,
  onCreated,
}: {
  loanType: LoanType | null
  onClose: () => void
  onCreated: () => void
}) {
  const { message } = App.useApp()
  const [form] = Form.useForm()

  const [quote, setQuote] = useState<LoanQuote | null>(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [amount, setAmount] = useState<number | null>(null)
  const [files, setFiles] = useState<File[]>([])

  const isTravel = loanType?.type === 'TravelLoan'
  const isMarriage = loanType?.type === 'MarriageLoan'
  const isSpecialCase = loanType?.type === 'SpecialCaseLoan'
  const isImmediate = loanType?.type === 'ImmediatePaymentLoan'

  // اگر تاریخ عقد در پروفایل ثبت شده باشد، فرم فقط نشانش می‌دهد؛ وگرنه می‌پرسد.
  const marriageDateOnFile = Boolean(quote?.marriageDate)

  useEffect(() => {
    if (!loanType) return

    setLoading(true)
    setQuote(null)
    setAmount(null)
    setFiles([])
    form.resetFields()

    getLoanQuote(loanType.id)
      .then(setQuote)
      .catch(() => message.error('خطا در دریافت اطلاعات وام.'))
      .finally(() => setLoading(false))
  }, [loanType, form, message])

  // با هر بار عوض شدن مبلغ، گزینه‌های اقساط از سرور تازه گرفته می‌شوند.
  useEffect(() => {
    if (!loanType || !amount) return

    getLoanQuote(loanType.id, amount)
      .then(setQuote)
      .catch(() => {})

    form.setFieldValue('installmentCount', undefined)
  }, [amount, loanType, form])

  if (!loanType) return null

  const money = (v: number) => `${Math.round(v).toLocaleString('fa-IR')} تومان`

  async function onFinish(values: {
    requestedAmount: number
    installmentCount: number
    destinationType?: string
    destination?: string
    travelDates?: [{ toISOString: () => string }, { toISOString: () => string }]
    marriageDate?: { toISOString: () => string }
    spouseFirstName?: string
    spouseLastName?: string
    spouseNationalId?: string
    specialCaseCategory?: string
    specialCaseDescription?: string
    immediatePaymentPurpose?: string
    notes?: string
  }) {
    if (quote?.requiresDocument && files.length === 0) {
      message.error(`بارگذاری ${quote.requiredDocumentDescription ?? 'مدرک'} الزامی است.`)
      return
    }

    setSubmitting(true)
    try {
      await createLoanRequest({
        loanTypeId: loanType!.id,
        requestedAmount: values.requestedAmount,
        installmentCount: values.installmentCount,
        destinationType: isTravel ? values.destinationType : undefined,
        destination: isTravel ? values.destination : undefined,
        startDate: isTravel ? values.travelDates?.[0].toISOString() : undefined,
        endDate: isTravel ? values.travelDates?.[1].toISOString() : undefined,
        // تاریخ عقد فقط وقتی فرستاده می‌شود که در پروفایل نباشد.
        marriageDate: isMarriage && !marriageDateOnFile
          ? values.marriageDate?.toISOString()
          : undefined,
        spouseFirstName: isMarriage ? values.spouseFirstName : undefined,
        spouseLastName: isMarriage ? values.spouseLastName : undefined,
        spouseNationalId: isMarriage ? values.spouseNationalId : undefined,
        specialCaseCategory: isSpecialCase ? values.specialCaseCategory : undefined,
        specialCaseDescription: isSpecialCase ? values.specialCaseDescription : undefined,
        immediatePaymentPurpose: isImmediate ? values.immediatePaymentPurpose : undefined,
        notes: values.notes,
        files,
      })

      message.success('درخواست وام با موفقیت ثبت شد و در انتظار بررسی ادمین است.')
      onCreated()
      onClose()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در ثبت درخواست وام.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Modal
      open
      onCancel={onClose}
      footer={null}
      width={880}
      style={{ top: 24 }}
      title={`درخواست ${loanType.name}`}
      destroyOnHidden
    >
      {loading ? (
        <div style={{ display: 'grid', placeItems: 'center', minHeight: 240 }}>
          <Spin size="large" />
        </div>
      ) : !quote?.isEligible ? (
        <Alert
          type="error"
          showIcon
          message="امکان درخواست این وام را ندارید"
          description={quote?.ineligibilityReason}
        />
      ) : (
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Row gutter={24}>
            {/* ───── ستون راست: اطلاعات وام ───── */}
            <Col xs={24} lg={14}>
              {isTravel && (
                <>
                  <Divider titlePlacement="start" style={{ marginTop: 0 }}>
                    اطلاعات سفر
                  </Divider>

                  <Row gutter={12}>
                    <Col xs={24} sm={10}>
                      <Form.Item
                        label="مقصد"
                        name="destinationType"
                        rules={[{ required: true, message: 'نوع مقصد را انتخاب کنید' }]}
                      >
                        <Select
                          placeholder="انتخاب کنید"
                          options={[
                            { value: 'Domestic', label: 'داخلی' },
                            { value: 'International', label: 'خارجی' },
                          ]}
                        />
                      </Form.Item>
                    </Col>
                    <Col xs={24} sm={14}>
                      <Form.Item
                        label="شهر یا کشور"
                        name="destination"
                        rules={[{ required: true, message: 'مقصد را وارد کنید' }]}
                      >
                        <Input placeholder="مثلاً مشهد یا ترکیه" />
                      </Form.Item>
                    </Col>
                  </Row>

                  <Form.Item
                    label="تاریخ شروع و پایان سفر"
                    name="travelDates"
                    rules={[{ required: true, message: 'تاریخ سفر را انتخاب کنید' }]}
                  >
                    <DatePicker.RangePicker
                      style={{ width: '100%' }}
                      placeholder={['شروع سفر', 'پایان سفر']}
                      // شروعِ سفر باید از فردا به بعد باشد؛ امروز و گذشته غیرفعال.
                      disabledDate={(current) =>
                        !!current && current < dayjs().add(1, 'day').startOf('day')
                      }
                    />
                  </Form.Item>
                </>
              )}

              {isMarriage && (
                <>
                  <Divider titlePlacement="start" style={{ marginTop: 0 }}>
                    اطلاعات ازدواج
                  </Divider>

                  {marriageDateOnFile ? (
                    <Alert
                      type="info"
                      showIcon
                      style={{ marginBottom: 16 }}
                      message={`تاریخ عقد ثبت‌شده: ${quote?.marriageDatePersian}`}
                      description="این تاریخ از پروفایل شما خوانده شده و از این فرم قابل تغییر نیست."
                    />
                  ) : (
                    <Form.Item
                      label="تاریخ عقد"
                      name="marriageDate"
                      rules={[{ required: true, message: 'تاریخ عقد را انتخاب کنید' }]}
                      extra="این تاریخ در پروفایل شما ذخیره می‌شود."
                    >
                      <DatePicker style={{ width: '100%' }} placeholder="تاریخ عقد" />
                    </Form.Item>
                  )}

                  <Row gutter={12}>
                    <Col xs={24} sm={12}>
                      <Form.Item
                        label="نام همسر"
                        name="spouseFirstName"
                        rules={[{ required: true, message: 'نام همسر را وارد کنید' }]}
                      >
                        <Input />
                      </Form.Item>
                    </Col>
                    <Col xs={24} sm={12}>
                      <Form.Item
                        label="نام خانوادگی همسر"
                        name="spouseLastName"
                        rules={[{ required: true, message: 'نام خانوادگی همسر را وارد کنید' }]}
                      >
                        <Input />
                      </Form.Item>
                    </Col>
                  </Row>

                  <Form.Item
                    label="کد ملی همسر"
                    name="spouseNationalId"
                    rules={[
                      { required: true, message: 'کد ملی همسر را وارد کنید' },
                      { pattern: /^\d{10}$/, message: 'کد ملی باید ۱۰ رقم باشد' },
                    ]}
                  >
                    <Input maxLength={10} style={{ direction: 'ltr', textAlign: 'left' }} placeholder="۱۰ رقم" />
                  </Form.Item>
                </>
              )}

              {isSpecialCase && (
                <>
                  <Divider titlePlacement="start" style={{ marginTop: 0 }}>
                    اطلاعات مورد
                  </Divider>

                  <Form.Item
                    label="دسته‌ی مورد"
                    name="specialCaseCategory"
                    rules={[{ required: true, message: 'دسته‌ی مورد را انتخاب کنید' }]}
                  >
                    <Select
                      placeholder="انتخاب کنید"
                      options={[
                        { value: 'Medical', label: 'درمانی' },
                        { value: 'Damage', label: 'خسارت مالی' },
                        { value: 'Bereavement', label: 'فوت بستگان' },
                        { value: 'Other', label: 'سایر' },
                      ]}
                    />
                  </Form.Item>

                  <Form.Item
                    label="شرح مورد"
                    name="specialCaseDescription"
                    rules={[{ required: true, message: 'شرح مورد را وارد کنید' }]}
                  >
                    <Input.TextArea rows={3} placeholder="لطفاً مورد خود را شرح دهید" />
                  </Form.Item>
                </>
              )}

              {isImmediate && (
                <Form.Item
                  label="بابتِ درخواست"
                  name="immediatePaymentPurpose"
                  rules={[{ required: true, message: 'بابتِ درخواست را انتخاب کنید' }]}
                  style={{ marginTop: 0 }}
                >
                  <Select
                    placeholder="انتخاب کنید"
                    options={[
                      { value: 'Treatment', label: 'هزینه‌ی درمان' },
                      { value: 'Repair', label: 'تعمیر ضروری' },
                      { value: 'Debt', label: 'تسویه‌ی بدهی' },
                      { value: 'Other', label: 'سایر' },
                    ]}
                  />
                </Form.Item>
              )}

              <Divider titlePlacement="start">مبلغ و اقساط</Divider>

              <Form.Item
                label="مبلغ درخواستی"
                name="requestedAmount"
                rules={[{ required: true, message: 'مبلغ را انتخاب کنید' }]}
                extra={`از ${money(quote.minAmount)} تا ${money(quote.maxAmount)}`}
              >
                <Select
                  placeholder="مبلغ را انتخاب کنید"
                  onChange={(v) => setAmount(Number(v))}
                  options={quote.amountOptions.map((a) => ({ value: a, label: money(a) }))}
                />
              </Form.Item>

              <Form.Item
                label="تعداد اقساط"
                name="installmentCount"
                rules={[{ required: true, message: 'تعداد اقساط را انتخاب کنید' }]}
                extra={
                  amount
                    ? 'مبلغ هر قسط با احتساب کارمزد محاسبه شده است.'
                    : 'ابتدا مبلغ را انتخاب کنید.'
                }
              >
                <Select
                  placeholder={amount ? 'انتخاب کنید' : 'ابتدا مبلغ را انتخاب کنید'}
                  disabled={!amount}
                  optionLabelProp="label"
                  options={quote.installmentOptions.map((o) => ({
                    value: o.installmentCount,
                    // گزینه‌های غیرقابل‌پرداخت حذف نمی‌شوند بلکه غیرفعال می‌شوند،
                    // تا کارمند ببیند چرا و با اقساط بیشتر امتحان کند.
                    disabled: !o.isAffordable,
                    label: `${o.installmentCount.toLocaleString('fa-IR')} قسط — ${money(o.monthlyPayment)} در ماه`,
                    title: '',
                    item: o,
                  }))}
                  optionRender={(opt) => {
                    const o = (opt.data as { item: (typeof quote.installmentOptions)[0] }).item
                    return (
                      <div
                        style={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'center',
                          gap: 12,
                        }}
                      >
                        <span>قسط {o.installmentCount.toLocaleString('fa-IR')} ماهه</span>
                        <span style={{ fontWeight: 600 }}>{money(o.monthlyPayment)} در ماه</span>
                        {!o.isAffordable && <Tag color="red">بیش از سقف حقوق</Tag>}
                      </div>
                    )
                  }}
                />
              </Form.Item>

              <Form.Item label="توضیحات" name="notes">
                <Input.TextArea rows={3} placeholder="توضیح اختیاری درباره‌ی این درخواست" />
              </Form.Item>
            </Col>

            {/* ───── ستون چپ: خلاصه و مدارک ───── */}
            <Col xs={24} lg={10}>
              <Card size="small" title="خلاصه" style={{ marginBottom: 16 }}>
                <SummaryRow label="سقف وام شما" value={money(quote.maxAmount)} />
                <SummaryRow
                  label="سقف قسط ماهانه"
                  value={money(quote.maxMonthlyInstallment)}
                />
                <SummaryRow
                  label="کارمزد"
                  value={
                    quote.annualFeePercent > 0
                      ? `${quote.annualFeePercent}٪ سالانه`
                      : 'بدون کارمزد'
                  }
                />

                {amount && (
                  <>
                    <Divider style={{ margin: '12px 0' }} />
                    <SummaryRow label="مبلغ انتخابی" value={money(amount)} strong />
                    <SelectedInstallmentSummary form={form} quote={quote} money={money} />
                  </>
                )}
              </Card>

              <Card size="small" title="مدارک">
                {quote.requiresDocument ? (
                  <Alert
                    type="warning"
                    showIcon
                    style={{ marginBottom: 12 }}
                    message={`${quote.requiredDocumentDescription} الزامی است`}
                    description="بدون بارگذاری حداقل یک فایل، امکان ثبت درخواست وجود ندارد."
                  />
                ) : (
                  <Alert
                    type="info"
                    style={{ marginBottom: 12 }}
                    message="بارگذاری مدرک برای این وام اختیاری است."
                  />
                )}

                <Upload
                  multiple
                  beforeUpload={(file) => {
                    setFiles((prev) =>
                      prev.length >= MAX_FILES ? prev : [...prev, file],
                    )
                    return false
                  }}
                  onRemove={(file) =>
                    setFiles((prev) => prev.filter((f) => f.name !== file.name))
                  }
                  accept=".jpg,.jpeg,.png,.pdf"
                  fileList={files.map((f, i) => ({
                    uid: String(i),
                    name: f.name,
                    status: 'done' as const,
                  }))}
                >
                  <Button icon={<UploadOutlined />} disabled={files.length >= MAX_FILES}>
                    انتخاب فایل
                  </Button>
                </Upload>

                <div style={{ color: 'var(--text-muted)', fontSize: 12, marginTop: 8 }}>
                  حداکثر {MAX_FILES.toLocaleString('fa-IR')} فایل — عکس یا PDF، هر ترکیبی.
                  حداکثر حجم هر فایل ۵ مگابایت.
                </div>
              </Card>
            </Col>
          </Row>

          <Divider />

          <Row gutter={12} justify="end">
            <Col>
              <Button onClick={onClose} disabled={submitting}>
                انصراف
              </Button>
            </Col>
            <Col>
              <Button type="primary" htmlType="submit" loading={submitting} size="large">
                ثبت درخواست وام
              </Button>
            </Col>
          </Row>
        </Form>
      )}
    </Modal>
  )
}

function SummaryRow({
  label,
  value,
  strong,
}: {
  label: string
  value: string
  strong?: boolean
}) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
      <span style={{ color: 'var(--text-muted)' }}>{label}</span>
      <span style={{ fontWeight: strong ? 700 : 500 }}>{value}</span>
    </div>
  )
}

/** خلاصه‌ی گزینه‌ی اقساطی که همین الان انتخاب شده. */
function SelectedInstallmentSummary({
  form,
  quote,
  money,
}: {
  form: { getFieldValue: (n: string) => unknown }
  quote: LoanQuote
  money: (v: number) => string
}) {
  const count = Form.useWatch('installmentCount', form as never) as number | undefined

  const option = quote.installmentOptions.find((o) => o.installmentCount === count)

  if (!option) return null

  return (
    <>
      <SummaryRow label="تعداد اقساط" value={`${count?.toLocaleString('fa-IR')} ماه`} />
      {option.totalFee > 0 && (
        <SummaryRow label="کارمزد کل" value={money(option.totalFee)} />
      )}
      <SummaryRow label="کل بازپرداخت" value={money(option.totalPayable)} />
      <SummaryRow label="قسط ماهانه" value={money(option.monthlyPayment)} strong />
    </>
  )
}
