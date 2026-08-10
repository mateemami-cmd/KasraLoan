using KasraLoan.Domain.Enums;
using System;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// یک تلاش برای پرداخت یک قسط.
    ///
    /// عمداً جدا از <see cref="LoanInstallment"/> است و نه چند فیلد اضافه روی آن:
    /// یک قسط ممکن است چکِ ردشده و پرداخت ناموفق داشته باشد و بعد تسویه شود.
    /// اگر روی خود قسط می‌نشست، هر تلاش تازه تلاش قبلی را پاک می‌کرد و رد
    /// حسابرسی از بین می‌رفت.
    ///
    /// نبودِ رکورد برای یک قسط یعنی کارمند هنوز روشی انتخاب نکرده است.
    /// </summary>
    public class InstallmentPayment
    {
        public Guid Id { get; set; }

        public Guid LoanInstallmentId { get; set; }

        public LoanInstallment LoanInstallment { get; set; } = null!;

        public Guid EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public PaymentMethod Method { get; set; }

        public InstallmentPaymentStatus Status { get; set; }

        /// <summary>مبلغی که این تلاش قرار است بپردازد.</summary>
        public decimal Amount { get; set; }

        // ───── چک ─────

        /// <summary>مسیر تصویر چک، از همان مسیری که مدارک وام ذخیره می‌شوند.</summary>
        public string? ChequeImageUrl { get; set; }

        public string? ChequeNumber { get; set; }

        public string? ChequeBankName { get; set; }

        /// <summary>
        /// تاریخ روی چک. چک لزوماً همان روزِ دریافت نقد نمی‌شود؛ این تاریخ نگه
        /// داشته می‌شود تا بعداً بشود مرحله‌ی «پاس شدن» را هم اضافه کرد.
        /// </summary>
        public DateTime? ChequeDate { get; set; }

        // ───── درگاه ─────

        /// <summary>شناسه‌ی نشستِ پرداخت که به درگاه داده می‌شود.</summary>
        public Guid? GatewayAuthority { get; set; }

        /// <summary>شماره پیگیری بازگشتی از درگاه.</summary>
        public string? GatewayRefId { get; set; }

        /// <summary>نشست پرداخت آنلاین بعد از این زمان بی‌اعتبار است.</summary>
        public DateTime? GatewayExpiresAt { get; set; }

        // ───── تأیید ─────

        public Guid? ConfirmedByAdminId { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
