using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string Token { get; set; } = string.Empty;

        public string JwtId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool Revoked { get; set; }

        public int? ReplacedByTokenId { get; set; }

        // ─── مشخصات نشست/دستگاه، برای صفحه‌ی «نشست‌های فعال» ───
        /// <summary>
        /// شناسه‌ی ثابتِ دستگاه/مرورگر (یک GUID که سمت کلاینت ساخته و ذخیره می‌شود).
        /// کلیدِ یکتاسازیِ نشست‌هاست: ورودِ دوباره با همین دستگاه، به‌جای ساختِ نشستِ
        /// جدید، همین ردیف را به‌روز می‌کند. پس یک دستگاه = یک نشست.
        /// </summary>
        public string? DeviceId { get; set; }

        /// <summary>سیستم‌عاملِ دستگاه (مثلاً "Windows"، "iOS").</summary>
        public string? DeviceOs { get; set; }

        /// <summary>مرورگرِ دستگاه (مثلاً "Chrome"، "Mobile Safari").</summary>
        public string? DeviceBrowser { get; set; }

        /// <summary>آدرس IP هنگام ورود.</summary>
        public string? IpAddress { get; set; }

        /// <summary>آخرین باری که این نشست فعالیت داشته (برای ستون «آخرین دسترسی»).</summary>
        public DateTime LastSeenAt { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}