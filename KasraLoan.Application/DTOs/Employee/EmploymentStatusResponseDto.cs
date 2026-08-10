using KasraLoan.Application.DTOs.Loans;
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

        /// <summary>آیا کارمند در لحظه‌ی تغییر وام تسویه‌نشده داشت.</summary>
        public bool HasOutstandingLoan { get; set; }

        /// <summary>
        /// اگر با پایان همکاری، تسویه‌ی یکجا مطالبه شده باشد: مبلغ و مهلت.
        /// در حالت بازگشت به کار یا نبودِ وامِ باز، null است.
        /// </summary>
        public LoanSettlementDemandDto? Settlement { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
