using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace KasraLoan.API.Models
{
    /// <summary>
    /// فرم multipart درخواست وام.
    ///
    /// چون multipart است، فیلدها تخت‌اند و نمی‌شود آبجکت تودرتو فرستاد؛ به همین
    /// دلیل فیلدهای سفر مستقیم اینجا آمده‌اند و در کنترلر به DTO تبدیل می‌شوند.
    /// </summary>
    public class CreateLoanRequestForm
    {
        public int LoanTypeId { get; set; }

        public long RequestedAmount { get; set; }

        public int InstallmentCount { get; set; }

        // ───── مخصوص وام سفر ─────

        /// <summary>"Domestic" یا "International"</summary>
        public string? DestinationType { get; set; }

        public string? Destination { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Notes { get; set; }

        // ───── مخصوص وام ازدواج ─────

        /// <summary>
        /// تاریخ عقد. فقط وقتی لازم است که در پروفایل کارمند ثبت نشده باشد؛
        /// تاریخ ثبت‌شده از این مسیر بازنویسی نمی‌شود.
        /// </summary>
        public DateTime? MarriageDate { get; set; }

        public string? SpouseFirstName { get; set; }

        public string? SpouseLastName { get; set; }

        public string? SpouseNationalId { get; set; }

        // ───── مخصوص وام موردی ─────

        /// <summary>"Medical" | "Damage" | "Bereavement" | "Other"</summary>
        public string? SpecialCaseCategory { get; set; }

        public string? SpecialCaseDescription { get; set; }

        /// <summary>مدارک؛ حداکثر دو فایل، ترکیب عکس و PDF آزاد است.</summary>
        public List<IFormFile>? Files { get; set; }

        /// <summary>آیا فرم اطلاعات سفر همراه دارد.</summary>
        public bool HasTravelDetails =>
            !string.IsNullOrWhiteSpace(Destination) || !string.IsNullOrWhiteSpace(DestinationType);

        /// <summary>آیا فرم اطلاعات مورد همراه دارد.</summary>
        public bool HasSpecialCaseDetails =>
            !string.IsNullOrWhiteSpace(SpecialCaseCategory)
            || !string.IsNullOrWhiteSpace(SpecialCaseDescription);

        /// <summary>آیا فرم اطلاعات ازدواج همراه دارد.</summary>
        public bool HasMarriageDetails =>
            !string.IsNullOrWhiteSpace(SpouseFirstName)
            || !string.IsNullOrWhiteSpace(SpouseLastName)
            || !string.IsNullOrWhiteSpace(SpouseNationalId)
            || MarriageDate.HasValue;
    }
}
