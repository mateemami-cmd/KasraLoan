using System;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// یک تلاشِ ورود (موفق یا ناموفق) برای صفحه‌ی «تاریخچه ورودهای اخیر».
    /// برخلافِ RefreshToken که فقط ورودِ موفق را نگه می‌دارد، اینجا تلاش‌های ناموفق
    /// (رمزِ اشتباه، حسابِ غیرفعال) هم ثبت می‌شود تا ستونِ «نتیجه» معنی داشته باشد.
    /// </summary>
    public class LoginHistory
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }

        public DateTime AttemptedAt { get; set; }

        public string? IpAddress { get; set; }

        /// <summary>سیستم‌عاملِ دستگاه (مثلاً "Windows 10.0").</summary>
        public string? DeviceOs { get; set; }

        /// <summary>مرورگرِ دستگاه (مثلاً "Chrome 151").</summary>
        public string? DeviceBrowser { get; set; }

        /// <summary>نتیجه‌ی تلاش: موفق (true) یا ناموفق (false).</summary>
        public bool IsSuccess { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
