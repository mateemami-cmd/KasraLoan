// نام باید فقط فارسی باشد: حروفِ فارسی/عربی + فاصله + نیم‌فاصله (ZWNJ). حروفِ
// انگلیسی، عدد و علائم مجاز نیستند. هم‌رفتار با PersianText در بک‌اند.
const PERSIAN_NAME = /^[؀-ۿ‌\s]+$/

export function isPersianName(value: string | null | undefined): boolean {
  const v = (value ?? '').trim()
  return v.length > 0 && PERSIAN_NAME.test(v)
}
