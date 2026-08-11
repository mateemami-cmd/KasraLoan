using System.Collections.Generic;

namespace KasraLoan.Application.DTOs.Loans
{
    /// <summary>
    /// آنچه فرم درخواست وام برای پر کردن لیست‌هایش لازم دارد.
    ///
    /// همه‌ی محاسبات اینجا سمت سرور انجام می‌شود تا فرم مجبور نباشد فرمول کارمزد
    /// و سقف را تکرار کند — تکرارِ فرمول یعنی روزی که قانون عوض شود، فرم عدد
    /// اشتباه نشان می‌دهد.
    /// </summary>
    public class LoanQuoteDto
    {
        public int LoanTypeId { get; set; }

        public string LoanTypeName { get; set; } = string.Empty;

        /// <summary>آیا کارمند اصلاً مجاز به گرفتن این وام هست.</summary>
        public bool IsEligible { get; set; }

        /// <summary>اگر مجاز نیست، دلیلش.</summary>
        public string? IneligibilityReason { get; set; }

        public long MinAmount { get; set; }

        /// <summary>سقف نهایی؛ کمینه‌ی سقف امتیاز، سقف نوع وام و سقف حقوق.</summary>
        public long MaxAmount { get; set; }

        /// <summary>فاصله‌ی گزینه‌های مبلغ در لیست کشویی.</summary>
        public long AmountStep { get; set; }

        /// <summary>گزینه‌های مبلغ، آماده برای نمایش در لیست.</summary>
        public List<long> AmountOptions { get; set; } = new();

        public decimal AnnualFeePercent { get; set; }

        public bool RequiresDocument { get; set; }

        public string? RequiredDocumentDescription { get; set; }

        /// <summary>سقف قسط ماهانه‌ی کارمند (یک‌سوم حقوق).</summary>
        public decimal MaxMonthlyInstallment { get; set; }

        /// <summary>
        /// گزینه‌های تعداد اقساط برای مبلغ انتخاب‌شده، هرکدام با قسط ماهانه.
        /// وقتی مبلغی ارسال نشده باشد خالی است.
        /// </summary>
        public List<InstallmentOptionDto> InstallmentOptions { get; set; } = new();
    }

    /// <summary>یک گزینه‌ی تعداد قسط، با مبلغ ماهانه‌ی محاسبه‌شده.</summary>
    public class InstallmentOptionDto
    {
        public int InstallmentCount { get; set; }

        /// <summary>مبلغ هر قسط، با احتساب کارمزد.</summary>
        public decimal MonthlyPayment { get; set; }

        /// <summary>اصل + کارمزد.</summary>
        public long TotalPayable { get; set; }

        public long TotalFee { get; set; }

        /// <summary>
        /// آیا این قسط از سقف حقوق کارمند عبور نمی‌کند.
        /// گزینه‌های غیرقابل‌پرداخت حذف نمی‌شوند بلکه علامت می‌خورند، تا کارمند
        /// ببیند چرا آن گزینه در دسترس نیست و با اقساط بیشتر امتحان کند.
        /// </summary>
        public bool IsAffordable { get; set; }
    }
}
