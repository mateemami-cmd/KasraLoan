using System;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// جزئیات مخصوص هر نوع وام، در یک ستون jsonb روی <see cref="LoanRequest"/>.
    ///
    /// چرا jsonb و نه ستون‌های جداگانه: با شش نوع وام، ستون‌گذاشتن یعنی چهل‌ ستون
    /// nullable که برای هر وام اکثرشان خالی‌اند. اینجا هر نوع وام زیرشاخه‌ی خودش
    /// را دارد و در کد هم تایپ‌سیف می‌ماند، چون EF Core به کلاس نگاشت می‌کند.
    /// </summary>
    public class LoanDetails
    {
        public TravelLoanDetails? Travel { get; set; }
    }

    /// <summary>اطلاعات تکمیلی وام سفر.</summary>
    public class TravelLoanDetails
    {
        /// <summary>مقصد داخلی است یا خارجی.</summary>
        public TravelDestinationType DestinationType { get; set; }

        /// <summary>شهر (برای سفر داخلی) یا کشور (برای سفر خارجی).</summary>
        public string Destination { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        /// <summary>طول سفر به روز؛ محاسبه‌شده، ذخیره نمی‌شود.</summary>
        public int DurationDays => Math.Max(0, (EndDate.Date - StartDate.Date).Days);
    }

    public enum TravelDestinationType
    {
        Domestic = 0,
        International = 1
    }
}
