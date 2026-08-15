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

        /// <summary>جزئیات مخصوص وام ازدواج.</summary>
        public MarriageDetailsDto? Marriage { get; set; }

        /// <summary>جزئیات مخصوص وام موردی.</summary>
        public SpecialCaseDetailsDto? SpecialCase { get; set; }
    }

    /// <summary>ورودی فرم وام موردی.</summary>
    public class SpecialCaseDetailsDto
    {
        /// <summary>"Medical" | "Damage" | "Bereavement" | "Other"</summary>
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    /// <summary>ورودی فرم وام ازدواج.</summary>
    public class MarriageDetailsDto
    {
        /// <summary>
        /// تاریخ عقد. اگر در پروفایل کارمند خالی باشد، از همین‌جا گرفته و در
        /// پروفایل ذخیره می‌شود؛ اگر از قبل ثبت شده باشد، همان معتبر است.
        /// </summary>
        public DateTime? MarriageDate { get; set; }

        public string SpouseFirstName { get; set; } = string.Empty;

        public string SpouseLastName { get; set; } = string.Empty;

        public string SpouseNationalId { get; set; } = string.Empty;

        public string? Notes { get; set; }
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