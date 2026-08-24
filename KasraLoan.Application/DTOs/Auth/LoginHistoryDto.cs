using System;

namespace KasraLoan.Application.DTOs.Auth
{
    /// <summary>یک ردیف از «تاریخچه ورودهای اخیر».</summary>
    public class LoginHistoryDto
    {
        public DateTime AttemptedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceOs { get; set; }
        public string? DeviceBrowser { get; set; }
        public bool IsSuccess { get; set; }
    }
}
