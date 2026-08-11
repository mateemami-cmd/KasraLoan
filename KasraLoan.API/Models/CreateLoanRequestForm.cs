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

        /// <summary>مدارک؛ حداکثر دو فایل، ترکیب عکس و PDF آزاد است.</summary>
        public List<IFormFile>? Files { get; set; }

        /// <summary>آیا فرم اطلاعات سفر همراه دارد.</summary>
        public bool HasTravelDetails =>
            !string.IsNullOrWhiteSpace(Destination) || !string.IsNullOrWhiteSpace(DestinationType);
    }
}
