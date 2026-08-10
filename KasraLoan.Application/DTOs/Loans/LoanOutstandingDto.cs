using System;

namespace KasraLoan.Application.DTOs.Loans
{
    /// <summary>وضعیت مانده‌ی یک وام.</summary>
    public class LoanOutstandingDto
    {
        public Guid LoanRequestId { get; set; }

        public long TotalPayableAmount { get; set; }

        public long PaidAmount { get; set; }

        /// <summary>جمع اقساط پرداخت‌نشده.</summary>
        public long OutstandingAmount { get; set; }

        public int TotalInstallments { get; set; }

        public int PaidInstallments { get; set; }

        public int RemainingInstallments { get; set; }

        /// <summary>آیا کل مانده یکجا مطالبه شده است.</summary>
        public bool IsSettlementDemanded { get; set; }

        public DateTime? SettlementDueDate { get; set; }

        public string? SettlementDueDatePersian { get; set; }

        public string? SettlementReason { get; set; }
    }
}
