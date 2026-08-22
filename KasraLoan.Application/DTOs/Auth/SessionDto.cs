using System;

namespace KasraLoan.Application.DTOs.Auth
{
    /// <summary>یک نشستِ فعالِ کاربر، برای نمایش در «نشست‌های فعال» و انتخابِ قطع.</summary>
    public class SessionDto
    {
        public int Id { get; set; }
        public string? DeviceOs { get; set; }
        public string? DeviceBrowser { get; set; }
        public string? IpAddress { get; set; }
        public DateTime LastSeenAt { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>آیا این همان نشستی است که همین حالا از آن درخواست آمده (نشست جاری).</summary>
        public bool IsCurrent { get; set; }
    }
}
