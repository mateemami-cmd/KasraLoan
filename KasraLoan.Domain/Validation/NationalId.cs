using System.Linq;

namespace KasraLoan.Domain.Validation
{
    /// <summary>
    /// اعتبارسنجی کد ملی ایران.
    ///
    /// فقط ۱۰ رقم بودن کافی نیست: رقم آخر، رقم کنترل است و از نه رقم اول
    /// محاسبه می‌شود. بدون این بررسی، «۱۲۳۴۵۶۷۸۹۰» هم قبول می‌شد.
    /// </summary>
    public static class NationalId
    {
        public static bool IsValid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var digits = Normalize(value);

            if (digits.Length != 10 || !digits.All(char.IsAsciiDigit))
                return false;

            // کدهایی مثل ۰۰۰۰۰۰۰۰۰۰ یا ۱۱۱۱۱۱۱۱۱۱ از نظر ریاضی رقم کنترلشان
            // درست درمی‌آید ولی معتبر نیستند.
            if (digits.Distinct().Count() == 1)
                return false;

            var sum = 0;

            for (var i = 0; i < 9; i++)
                sum += (digits[i] - '0') * (10 - i);

            var remainder = sum % 11;

            var checkDigit = digits[9] - '0';

            return remainder < 2
                ? checkDigit == remainder
                : checkDigit == 11 - remainder;
        }

        /// <summary>ارقام فارسی و عربی را به اسکی تبدیل می‌کند و جداکننده‌ها را برمی‌دارد.</summary>
        public static string Normalize(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];

                if (c >= '۰' && c <= '۹')
                    chars[i] = (char)(c - '۰' + '0');
                else if (c >= '٠' && c <= '٩')
                    chars[i] = (char)(c - '٠' + '0');
            }

            return new string(chars.Where(char.IsAsciiDigit).ToArray());
        }
    }
}
