import { useEffect, useState } from 'react'
import { Modal, Form, Input, Select, DatePicker, Switch, InputNumber, Row, Col, App } from 'antd'
import dayjs from 'dayjs'
import { getJobPositions, updateEmployeeByAdmin } from '../../api/services'
import type { JobPosition, UpdateEmployeePayload } from '../../api/services'
import { isValidNationalId } from '../../utils/nationalId'
import { isPersianName } from '../../utils/persian'

/** فیلدهایی که برای پرکردنِ فرمِ ویرایش لازم است. */
export interface EditableEmployee {
  id: string
  firstName: string
  lastName: string
  username: string
  personnelNumber: string
  nationalId?: string | null
  phoneNumber?: string | null
  email?: string | null
  hireDate?: string | null
  marriageDate?: string | null
  role: string
  isActive: boolean
  jobPositionId?: number | null
  monthlySalary?: number | null
}

/**
 * ویرایشِ کاملِ اطلاعاتِ یک کارمند یا ادمین. فقط برای ادمینِ ارشد (این مودال داخلِ
 * بخشی رندر می‌شود که خودش پشتِ isSenior است؛ بک‌اند هم فقط ادمینِ ارشد را می‌پذیرد).
 */
export function EditEmployeeModal({
  employee,
  onClose,
  onSaved,
}: {
  employee: EditableEmployee | null
  onClose: () => void
  onSaved: (id: string, patch: Partial<EditableEmployee>) => void
}) {
  const { message } = App.useApp()
  const [form] = Form.useForm()
  const [saving, setSaving] = useState(false)
  const [positions, setPositions] = useState<JobPosition[]>([])
  const role = Form.useWatch('role', form)

  useEffect(() => {
    getJobPositions().then(setPositions).catch(() => {})
  }, [])

  // با باز شدن برای یک کارمند، فرم را با مقادیرِ فعلی پر می‌کنیم.
  useEffect(() => {
    if (!employee) return
    form.setFieldsValue({
      firstName: employee.firstName,
      lastName: employee.lastName,
      nationalId: employee.nationalId ?? '',
      username: employee.username,
      personnelNumber: employee.personnelNumber,
      phoneNumber: employee.phoneNumber ?? undefined,
      email: employee.email ?? undefined,
      role: employee.role,
      isActive: employee.isActive,
      jobPositionId: employee.jobPositionId ?? undefined,
      monthlySalary: employee.monthlySalary ?? undefined,
      hireDate: employee.hireDate ? dayjs(employee.hireDate) : undefined,
      marriageDate: employee.marriageDate ? dayjs(employee.marriageDate) : undefined,
    })
  }, [employee, form])

  async function onFinish(values: {
    firstName: string
    lastName: string
    nationalId: string
    username: string
    personnelNumber: string
    phoneNumber?: string
    email?: string
    role: string
    isActive: boolean
    jobPositionId?: number
    monthlySalary?: number
    hireDate: { toISOString: () => string }
    marriageDate?: { toISOString: () => string }
  }) {
    if (!employee) return
    setSaving(true)
    try {
      const payload: UpdateEmployeePayload = {
        firstName: values.firstName,
        lastName: values.lastName,
        nationalId: values.nationalId,
        username: values.username,
        personnelNumber: values.personnelNumber,
        phoneNumber: values.phoneNumber || null,
        email: values.email || null,
        role: values.role,
        isActive: values.isActive,
        jobPositionId: values.jobPositionId ?? null,
        monthlySalary: values.monthlySalary ?? null,
        hireDate: values.hireDate.toISOString(),
        marriageDate: values.marriageDate ? values.marriageDate.toISOString() : null,
      }
      await updateEmployeeByAdmin(employee.id, payload)
      message.success('اطلاعات کاربر به‌روزرسانی شد.')
      onSaved(employee.id, {
        firstName: payload.firstName,
        lastName: payload.lastName,
        nationalId: payload.nationalId,
        username: payload.username,
        personnelNumber: payload.personnelNumber,
        phoneNumber: payload.phoneNumber,
        email: payload.email,
        role: payload.role,
        isActive: payload.isActive,
        jobPositionId: payload.jobPositionId,
        monthlySalary: payload.monthlySalary,
        hireDate: payload.hireDate,
        marriageDate: payload.marriageDate,
      })
      onClose()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { message?: string } } }
      message.error(e.response?.data?.message ?? 'خطا در به‌روزرسانی اطلاعات.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      open={!!employee}
      onCancel={onClose}
      onOk={() => form.submit()}
      confirmLoading={saving}
      title="ویرایش اطلاعات کاربر"
      okText="ذخیره"
      cancelText="انصراف"
      centered
      width={640}
      destroyOnHidden
    >
      <Form form={form} layout="vertical" onFinish={onFinish} requiredMark={false}>
        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="نام"
              name="firstName"
              rules={[
                { required: true, message: 'نام را وارد کنید' },
                {
                  validator: (_, v) =>
                    !v || isPersianName(v)
                      ? Promise.resolve()
                      : Promise.reject(new Error('نام باید فارسی باشد')),
                },
              ]}
            >
              <Input />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="نام خانوادگی"
              name="lastName"
              rules={[
                { required: true, message: 'نام خانوادگی را وارد کنید' },
                {
                  validator: (_, v) =>
                    !v || isPersianName(v)
                      ? Promise.resolve()
                      : Promise.reject(new Error('نام خانوادگی باید فارسی باشد')),
                },
              ]}
            >
              <Input />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="کد ملی"
              name="nationalId"
              rules={[
                { required: true, message: 'کد ملی را وارد کنید' },
                {
                  validator: (_, value) =>
                    !value || isValidNationalId(value)
                      ? Promise.resolve()
                      : Promise.reject(new Error('کد ملی معتبر نیست (۱۰ رقم با رقمِ کنترلیِ درست).')),
                },
              ]}
            >
              <Input maxLength={10} inputMode="numeric" style={{ direction: 'ltr', textAlign: 'right' }} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="نام کاربری"
              name="username"
              rules={[
                { required: true, message: 'نام کاربری را وارد کنید' },
                { pattern: /^[a-zA-Z0-9._-]+$/, message: 'فقط حروف انگلیسی، عدد، نقطه، خط تیره و آندرلاین' },
              ]}
            >
              <Input style={{ direction: 'ltr', textAlign: 'right' }} />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="شماره پرسنلی"
              name="personnelNumber"
              rules={[{ required: true, message: 'شماره پرسنلی را وارد کنید' }]}
            >
              <Input style={{ direction: 'ltr', textAlign: 'right' }} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label="شماره تماس"
              name="phoneNumber"
              rules={[{ pattern: /^09\d{9}$/, message: 'شماره موبایل معتبر نیست (مثال: 09123456789)' }]}
            >
              <Input placeholder="اختیاری" style={{ direction: 'ltr', textAlign: 'right' }} />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item label="ایمیل" name="email" rules={[{ type: 'email', message: 'ایمیل معتبر نیست' }]}>
              <Input placeholder="اختیاری" style={{ direction: 'ltr', textAlign: 'right' }} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="نقش" name="role" rules={[{ required: true }]}>
              <Select
                options={[
                  { value: 'Employee', label: 'کارمند' },
                  { value: 'Admin', label: 'ادمین' },
                ]}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="سمت شغلی"
              name="jobPositionId"
              rules={[
                {
                  required: role === 'Employee',
                  message: 'برای کارمند، سمت شغلی الزامی است',
                },
              ]}
            >
              <Select
                allowClear
                placeholder={role === 'Admin' ? 'اختیاری' : 'انتخاب کنید'}
                options={positions.map((p) => ({ value: p.id, label: p.title }))}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="حقوق اختصاصی (تومان)" name="monthlySalary">
              <InputNumber<number>
                style={{ width: '100%' }}
                placeholder="اختیاری"
                min={0}
                formatter={(v) => (v ? `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',') : '')}
                parser={(v) => (v ? Number(v.replace(/,/g, '')) : 0)}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item label="تاریخ استخدام" name="hireDate" rules={[{ required: true, message: 'تاریخ استخدام را وارد کنید' }]}>
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="تاریخ عقد" name="marriageDate">
              <DatePicker style={{ width: '100%' }} placeholder="اختیاری" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item label="حساب کاربری فعال" name="isActive" valuePropName="checked">
          <Switch checkedChildren="فعال" unCheckedChildren="غیرفعال" />
        </Form.Item>
      </Form>
    </Modal>
  )
}
