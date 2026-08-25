// اعتبارسنجیِ کد ملیِ ایران در سمتِ کلاینت — دقیقاً هم‌رفتار با NationalIdValidator
// در بک‌اند (checksum رسمی + پشتیبانی از ارقام فارسی/عربی).

/** ارقام فارسی/عربی را به لاتین تبدیل و هر چیزِ غیرعددی را حذف می‌کند. */
export function normalizeNationalId(input: string | null | undefined): string {
  let out = ''
  for (const ch of input ?? '') {
    const c = ch.charCodeAt(0)
    if (ch >= '0' && ch <= '9') out += ch
    else if (c >= 0x06f0 && c <= 0x06f9) out += String.fromCharCode(48 + (c - 0x06f0)) // ۰-۹
    else if (c >= 0x0660 && c <= 0x0669) out += String.fromCharCode(48 + (c - 0x0660)) // ٠-٩
  }
  return out
}

/** فقط ۱۰ رقم (بدونِ بررسیِ رقمِ کنترلی) — برای مقایسه/فراموشیِ رمز. */
export function hasTenDigits(input: string | null | undefined): boolean {
  return normalizeNationalId(input).length === 10
}

/** کد ملیِ ساختاراً معتبر (۱۰ رقم + رقمِ کنترلیِ درست). */
export function isValidNationalId(input: string | null | undefined): boolean {
  const code = normalizeNationalId(input)
  if (code.length !== 10) return false
  if (/^(\d)\1{9}$/.test(code)) return false // ارقامِ یکسان
  let sum = 0
  for (let i = 0; i < 9; i++) sum += Number(code[i]) * (10 - i)
  const r = sum % 11
  const check = Number(code[9])
  return (r < 2 && check === r) || (r >= 2 && check === 11 - r)
}
