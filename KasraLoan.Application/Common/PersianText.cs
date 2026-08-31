using System.Text.RegularExpressions;

namespace KasraLoan.Application.Common
{
    /// <summary>
    /// اعتبارسنجیِ متنِ فارسی. برای «نام» فقط حروفِ فارسی/عربی + فاصله + نیم‌فاصله
    /// (ZWNJ) مجاز است؛ حروفِ انگلیسی، اعداد و علائم رد می‌شوند تا نام حتماً فارسی باشد.
    /// ؀-ۿ بلوکِ فارسی/عربی است، ‌ نیم‌فاصله، و \s فاصله.
    /// </summary>
    public static class PersianText
    {
        private static readonly Regex PersianNameRegex =
            new Regex(@"^[؀-ۿ‌\s]+$", RegexOptions.Compiled);

        public static bool IsPersianName(string? value)
            => !string.IsNullOrWhiteSpace(value) && PersianNameRegex.IsMatch(value.Trim());
    }
}
