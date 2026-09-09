using System.Linq;
using System.Text;

namespace KasraLoan.Application.Common
{
    /// <summary>
    /// اعتبارسنجیِ ساختاریِ کد ملیِ ایران: ۱۰ رقم + رقمِ کنترلی (checksum رسمی).
    /// این فقط «خوش‌فرم و معتبر بودنِ خودِ عدد» را می‌سنجد (تایپوها و اعدادِ ساختگی را
    /// رد می‌کند)، نه وجودِ واقعیِ فرد در ثبت احوال؛ آن کار به استعلامِ آنلاین نیاز دارد.
    /// </summary>
    public static class NationalIdValidator
    {
        /// <summary>
        /// ارقامِ فارسی (۰-۹) و عربی (٠-٩) را به لاتین تبدیل و هر چیزِ غیرعددی (فاصله،
        /// خط تیره، …) را حذف می‌کند تا مقایسه/ذخیره یکدست باشد.
        /// </summary>
        public static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                if (ch >= '0' && ch <= '9') sb.Append(ch);
                else if (ch >= '۰' && ch <= '۹') sb.Append((char)('0' + (ch - '۰'))); // ۰-۹
                else if (ch >= '٠' && ch <= '٩') sb.Append((char)('0' + (ch - '٠'))); // ٠-٩
                // بقیه نادیده گرفته می‌شوند
            }
            return sb.ToString();
        }

        /// <summary>آیا فقط ۱۰ رقم است (بدونِ بررسیِ رقمِ کنترلی)؟ — برای مقایسه/جست‌وجو.</summary>
        public static bool HasTenDigits(string? input) => Normalize(input).Length == 10;

        /// <summary>
        /// آیا کد ملی قابل‌قبول است: فقط ۱۰ رقم و نه همه‌ارقام‌یکسان (مثل 1111111111).
        /// فعلاً رقمِ کنترلیِ رسمی بررسی نمی‌شود (به‌درخواستِ کارفرما برای سهولتِ ثبت).
        /// </summary>
        public static bool IsValid(string? input)
        {
            var code = Normalize(input);
            if (code.Length != 10) return false;

            // ارقامِ یکسان (مثل 1111111111) نامعتبرند.
            if (code.Distinct().Count() == 1) return false;

            return true;
        }
    }
}
