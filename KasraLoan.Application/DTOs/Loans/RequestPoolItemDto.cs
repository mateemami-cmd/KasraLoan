using System;

namespace KasraLoan.Application.DTOs.Loans
{
    /// <summary>
    /// یک ردیف از «استخرِ درخواست‌ها» — نمای یکپارچه‌ی همه‌ی درخواست‌هایی که
    /// کارمندها ثبت کرده‌اند (وام و مجوزِ وام)، برای ادمین ارشد. فقط‌خواندنی.
    /// </summary>
    public class RequestPoolItemDto
    {
        public Guid Id { get; set; }

        /// <summary>"Loan" یا "Permission".</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>برچسب فارسی نوع درخواست، مثل «درخواست وام» یا «درخواست مجوز وام».</summary>
        public string CategoryLabel { get; set; } = string.Empty;

        public int LoanTypeId { get; set; }
        public string LoanTypeName { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeUsername { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>خلاصه: برای وام مبلغ درخواستی، برای مجوز دلیلِ درخواست.</summary>
        public string? Detail { get; set; }
    }
}
