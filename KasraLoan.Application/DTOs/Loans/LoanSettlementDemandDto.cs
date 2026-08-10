using System;
using System.Collections.Generic;

namespace KasraLoan.Application.DTOs.Loans
{
    /// <summary>نتیجه‌ی مطالبه‌ی تسویه‌ی یکجا برای یک کارمند.</summary>
    public class LoanSettlementDemandDto
    {
        public List<Guid> LoanRequestIds { get; set; } = new();

        /// <summary>جمع مانده‌ی همه‌ی وام‌های باز.</summary>
        public long TotalOutstandingAmount { get; set; }

        public int RemainingInstallments { get; set; }

        public DateTime SettlementDueDate { get; set; }

        public string SettlementDueDatePersian { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
