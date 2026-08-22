using System;
using System.Text.RegularExpressions;

namespace KasraLoan.Application.Common
{
    public record DeviceInfo(string Os, string Browser);

    /// <summary>
    /// یک پارسِ سبک و بدون کتابخانه‌ی بیرونی از User-Agent، همراه با شماره‌ی نسخه،
    /// برای نمایش در صفحه‌ی «نشست‌های فعال» (مثل «Windows 10.0» و «Chrome 139»).
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

            // نسخه را با regex درمی‌آورد؛ «_» را به «.» تبدیل می‌کند (مثل iOS).
            string Ver(string pattern)
            {
                var m = Regex.Match(ua, pattern, RegexOptions.IgnoreCase);
                return m.Success ? m.Groups[1].Value.Replace('_', '.') : string.Empty;
            }

            string WithVer(string name, string ver) =>
                string.IsNullOrEmpty(ver) ? name : $"{name} {ver}";

            // نسخه‌ها کامل (با همه‌ی بخش‌ها) گرفته می‌شوند، مثل «139.0.0.0» یا «18.5.1».
            string os;
            if (Has("Windows"))
                os = WithVer("Windows", Ver(@"Windows NT (\d+(?:\.\d+)*)"));
            else if (Has("iPhone") || Has("iPad") || Has("iOS"))
                os = WithVer("iOS", Ver(@"OS (\d+(?:[_.]\d+)*)"));
            else if (Has("Android"))
                os = WithVer("Android", Ver(@"Android (\d+(?:\.\d+)*)"));
            else if (Has("Mac OS") || Has("Macintosh"))
                os = WithVer("macOS", Ver(@"Mac OS X (\d+(?:[_.]\d+)*)"));
            else if (Has("Linux"))
                os = "Linux";
            else
                os = Unknown;

            // ترتیب مهم است: Edge و Opera در User-Agent خود «Chrome» هم دارند،
            // و کروم «Safari» دارد؛ پس از خاص‌ترین به عام‌ترین بررسی می‌کنیم.
            string browser;
            if (Has("Edg"))
                browser = WithVer("Edge", Ver(@"Edg[A-Za-z]*/(\d+(?:\.\d+)*)"));
            else if (Has("OPR") || Has("Opera"))
                browser = WithVer("Opera", Ver(@"(?:OPR|Opera)/(\d+(?:\.\d+)*)"));
            else if (Has("CriOS"))
                browser = WithVer("Chrome", Ver(@"CriOS/(\d+(?:\.\d+)*)"));
            else if (Has("Chrome"))
                browser = WithVer("Chrome", Ver(@"Chrome/(\d+(?:\.\d+)*)"));
            else if (Has("Firefox") || Has("FxiOS"))
                browser = WithVer("Firefox", Ver(@"(?:Firefox|FxiOS)/(\d+(?:\.\d+)*)"));
            else if (Has("Mobile") && Has("Safari"))
                browser = WithVer("Mobile Safari", Ver(@"Version/(\d+(?:\.\d+)*)"));
            else if (Has("Safari"))
                browser = WithVer("Safari", Ver(@"Version/(\d+(?:\.\d+)*)"));
            else
                browser = Unknown;

            return new DeviceInfo(os, browser);
        }
    }
}
