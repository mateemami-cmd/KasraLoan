using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Loans
{
    public class CreateLoanRequestDto
    {
        public int LoanTypeId { get; set; }

        public long RequestedAmount { get; set; }

        public int InstallmentCount { get; set; }

        /// <summary>
        /// جزئیات مخصوص وام سفر. فقط وقتی نوع وام سفر باشد خوانده می‌شود.
        /// </summary>
        public TravelDetailsDto? Travel { get; set; }
    }

    /// <summary>ورودی فرم وام سفر.</summary>
    public class TravelDetailsDto
    {
        /// <summary>"Domestic" یا "International"</summary>
        public string DestinationType { get; set; } = string.Empty;

        /// <summary>شهر (سفر داخلی) یا کشور (سفر خارجی).</summary>
        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }
    }
}