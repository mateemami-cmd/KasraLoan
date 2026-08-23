// شناسه‌ی ثابتِ این مرورگر/دستگاه. یک‌بار ساخته می‌شود و در localStorage می‌ماند
// تا ورودهای بعدی «همان دستگاه» شناخته شوند و نشستِ تکراری ساخته نشود.
// (همان کاری که سایت‌های بزرگ برای «دستگاه‌های فعال» می‌کنند.)
const DEVICE_ID_KEY = 'deviceId'

export function getDeviceId(): string {
  let id = localStorage.getItem(DEVICE_ID_KEY)
  if (!id) {
    id =
      typeof crypto !== 'undefined' && 'randomUUID' in crypto
        ? crypto.randomUUID()
        : `${Date.now()}-${Math.random().toString(16).slice(2)}`
    localStorage.setItem(DEVICE_ID_KEY, id)
  }
  return id
}
