using System;

namespace KasraLoan.Application.Common
{
    public record DeviceInfo(string Os, string Browser);

    /// <summary>
    /// یک پارسِ سبک و بدون کتابخانه‌ی بیرونی از User-Agent، فقط برای نمایش در
    /// صفحه‌ی «نشست‌های فعال». دقتش در حدِ کافی برای این‌که کاربر دستگاه‌ها را
    /// از هم تشخیص دهد؛ نه بیشتر.
    /// </summary>
    public static class DeviceInfoParser
    {
        private const string Unknown = "نامشخص";

        public static DeviceInfo Parse(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return new DeviceInfo(Unknown, Unknown);

            var ua = userAgent;

            bool Has(string s) => ua.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;

            string os =
                Has("Windows") ? "Windows" :
                Has("iPhone") || Has("iPad") || Has("iOS") ? "iOS" :
                Has("Android") ? "Android" :
                Has("Mac OS") || Has("Macintosh") ? "macOS" :
                Has("Linux") ? "Linux" :
                Unknown;

            // ترتیب مهم است: Edge و Opera در User-Agent خود «Chrome» هم دارند،
            // و کروم «Safari» دارد؛ پس از خاص‌ترین به عام‌ترین بررسی می‌کنیم.
            string browser =
                Has("Edg") ? "Edge" :
                Has("OPR") || Has("Opera") ? "Opera" :
                Has("CriOS") ? "Chrome" :
                Has("Chrome") ? "Chrome" :
                Has("Firefox") || Has("FxiOS") ? "Firefox" :
                Has("Mobile") && Has("Safari") ? "Mobile Safari" :
                Has("Safari") ? "Safari" :
                Unknown;

            return new DeviceInfo(os, browser);
        }
    }
}
