using System;

namespace KasraLoan.Application.DTOs.Employee
{
    public class EmploymentStatusResponseDto
    {
        public Guid EmployeeId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? TerminationDate { get; set; }

        /// <summary>تاریخ شمسی ثبت تغییر، برای نمایش.</summary>
        public string ChangedAtPersian { get; set; } = string.Empty;

        /// <summary>
        /// آیا کارمند در لحظه‌ی تغییر وام تسویه‌نشده داشت. اگر true باشد،
        /// اقساط سرجایشان می‌مانند و کارمند همچنان می‌تواند وارد سیستم شود.
        /// </summary>
        public bool HasOutstandingLoan { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
