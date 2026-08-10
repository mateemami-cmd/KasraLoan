using System;
using System.Collections.Generic;

namespace KasraLoan.Application.DTOs.Loans
{
    /// <summary>انتخاب روش پرداخت برای یک قسط.</summary>
    public class SelectPaymentMethodRequestDto
    {
        /// <summary>"PayrollDeduction" | "OnlineGateway" | "Cheque"</summary>
        public string Method { get; set; } = string.Empty;
    }

    /// <summary>اطلاعات چک، هنگام بارگذاری تصویر.</summary>
    public class SubmitChequeRequestDto
    {
        public string ChequeNumber { get; set; } = string.Empty;

        public string ChequeBankName { get; set; } = string.Empty;

        public DateTime ChequeDate { get; set; }
    }

    /// <summary>
    /// ورودی صفحه‌ی پرداخت نمادین.
    /// این DTO داده‌ی حساس دارد؛ کامندی که آن را حمل می‌کند باید
    /// <c>ISensitiveRequest</c> باشد تا در لاگ نوشته نشود.
    /// </summary>
    public class GatewayPaymentRequestDto
    {
        public string CardNumber { get; set; } = string.Empty;

        public string Cvv2 { get; set; } = string.Empty;

        public string ExpiryMonth { get; set; } = string.Empty;

        public string ExpiryYear { get; set; } = string.Empty;

        public string SecondPassword { get; set; } = string.Empty;
    }

    /// <summary>یک تلاش پرداخت، برای نمایش.</summary>
    public class InstallmentPaymentDto
    {
        public Guid Id { get; set; }

        public Guid LoanInstallmentId { get; set; }

        public int InstallmentNumber { get; set; }

        public string Method { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string? ChequeImageUrl { get; set; }

        public string? ChequeNumber { get; set; }

        public string? ChequeBankName { get; set; }

        public DateTime? ChequeDate { get; set; }

        public string? ChequeDatePersian { get; set; }

        public string? GatewayRefId { get; set; }

        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; }

        // فقط در صف ادمین پر می‌شوند.
        public string? EmployeeName { get; set; }

        public string? LoanTypeName { get; set; }
    }

    /// <summary>نشستِ پرداخت آنلاین که به صفحه‌ی درگاه داده می‌شود.</summary>
    public class GatewaySessionDto
    {
        public Guid Authority { get; set; }

        public decimal Amount { get; set; }

        public int InstallmentNumber { get; set; }

        public string GatewayName { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        /// <summary>مسیری که کارمند باید به آن هدایت شود.</summary>
        public string RedirectUrl { get; set; } = string.Empty;
    }

    /// <summary>وضعیت قسط جاری و پنجره‌ی انتخاب روش پرداخت.</summary>
    public class CurrentInstallmentDto
    {
        public bool HasDueInstallment { get; set; }

        public Guid? LoanInstallmentId { get; set; }

        public int InstallmentNumber { get; set; }

        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public string? DueDatePersian { get; set; }

        /// <summary>آیا الان می‌شود روش پرداخت را انتخاب یا عوض کرد.</summary>
        public bool IsSelectionWindowOpen { get; set; }

        /// <summary>روشی که انتخاب شده؛ اگر چیزی انتخاب نشده باشد null است.</summary>
        public string? SelectedMethod { get; set; }

        public string? PaymentStatus { get; set; }

        public string WindowDescription { get; set; } = string.Empty;
    }
}
