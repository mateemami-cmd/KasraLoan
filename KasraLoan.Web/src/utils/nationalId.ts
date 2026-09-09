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

/**
 * کد ملیِ قابل‌قبول: فقط ۱۰ رقم و نه همه‌ارقام‌یکسان (مثل 1111111111).
 * فعلاً رقمِ کنترلیِ رسمی بررسی نمی‌شود (به‌درخواستِ کارفرما برای سهولتِ ثبت).
 */
export function isValidNationalId(input: string | null | undefined): boolean {
  const code = normalizeNationalId(input)
  if (code.length !== 10) return false
  if (/^(\d)\1{9}$/.test(code)) return false // ارقامِ یکسان
  return true
}
