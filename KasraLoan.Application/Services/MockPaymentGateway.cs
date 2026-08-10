using KasraLoan.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KasraLoan.Application.Services
{
    /// <summary>
    /// درگاه نمادین برای دمو و تست. هیچ تراکنش واقعی‌ای انجام نمی‌دهد.
    ///
    /// قاعده‌ها عمداً ساده‌اند:
    ///   • رمز دوم باید دقیقاً ۶ رقم باشد — تنها چیزی که واقعاً چک می‌شود
    ///   • شماره کارت باید ۱۶ رقم باشد (فقط برای این‌که صفحه واقعی به نظر برسد)
    ///   • رمز دومی که با ۰ شروع شود، عمداً ناموفق می‌شود تا بشود مسیر شکست را
    ///     هم در دمو نشان داد، نه فقط حالت خوش‌بینانه
    ///
    /// هیچ‌کدام از این مقادیر ذخیره یا لاگ نمی‌شوند.
    /// </summary>
    public class MockPaymentGateway : IPaymentGateway
    {
        // عمداً [0-9] و نه \d: در دات‌نت \d ارقام فارسی و عربی را هم می‌گیرد،
        // پس "۱۲۳۴۵۶" از اعتبارسنجی رد می‌شد بدون این‌که واقعاً عدد اسکی باشد.
        // ورودی اول نرمال‌سازی می‌شود، بعد با همین الگو سنجیده می‌شود.
        private static readonly Regex SixDigits = new("^[0-9]{6}$", RegexOptions.Compiled);
        private static readonly Regex SixteenDigits = new("^[0-9]{16}$", RegexOptions.Compiled);

        public string Name => "درگاه آزمایشی";

        public GatewayResult Authorize(GatewayCardInput input)
        {
            var cardNumber = KeepAsciiDigits(NormalizeDigits(input.CardNumber));

            if (!SixteenDigits.IsMatch(cardNumber))
                return GatewayResult.Failure("شماره کارت باید ۱۶ رقم باشد.");

            if (string.IsNullOrWhiteSpace(input.Cvv2))
                return GatewayResult.Failure("CVV2 را وارد کنید.");

            if (string.IsNullOrWhiteSpace(input.ExpiryMonth) ||
                string.IsNullOrWhiteSpace(input.ExpiryYear))
                return GatewayResult.Failure("تاریخ انقضای کارت را وارد کنید.");

            // کاربر ایرانی ممکن است با کیبورد فارسی رمز را بزند؛ ارقام فارسی و
            // عربی به اسکی تبدیل می‌شوند، ولی هر چیز دیگری همچنان رد می‌شود.
            var secondPassword = NormalizeDigits(input.SecondPassword);

            if (!SixDigits.IsMatch(secondPassword))
                return GatewayResult.Failure("رمز دوم باید دقیقاً ۶ رقم عدد باشد.");

            // مسیر شکستِ قابل نمایش در دمو.
            if (secondPassword.StartsWith('0'))
                return GatewayResult.Failure("تراکنش از سوی بانک تأیید نشد.");

            return GatewayResult.Success();
        }

        /// <summary>ارقام فارسی (۰-۹) و عربی (٠-٩) را به اسکی تبدیل می‌کند.</summary>
        private static string NormalizeDigits(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];

                if (c >= '۰' && c <= '۹')       // ۰-۹ فارسی
                    chars[i] = (char)(c - '۰' + '0');
                else if (c >= '٠' && c <= '٩')  // ٠-٩ عربی
                    chars[i] = (char)(c - '٠' + '0');
            }

            return new string(chars);
        }

        public string GenerateReferenceId()
        {
            // شماره پیگیری ۱۲ رقمی، شبیه چیزی که درگاه‌های واقعی برمی‌گردانند.
            return Random.Shared.NextInt64(100_000_000_000, 999_999_999_999).ToString();
        }

        /// <summary>
        /// جداکننده‌های شماره کارت (فاصله و خط تیره) را برمی‌دارد.
        /// عمداً char.IsDigit نیست، چون آن هم ارقام غیر‌اسکی را مجاز می‌شمارد.
        /// </summary>
        private static string KeepAsciiDigits(string value)
        {
            return new string(value.Where(c => c >= '0' && c <= '9').ToArray());
        }
    }
}
