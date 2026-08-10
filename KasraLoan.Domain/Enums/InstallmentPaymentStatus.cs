namespace KasraLoan.Domain.Enums
{
    /// <summary>
    /// وضعیت یک تلاش برای پرداخت قسط.
    ///
    /// روی «تلاش» تعریف شده، نه روی خود قسط: یک قسط ممکن است چند بار پرداخت
    /// ناموفق یا چکِ ردشده داشته باشد و بعد بالاخره تسویه شود. تاریخچه‌ی همه‌شان
    /// باید بماند، به‌خصوص وقتی پول واقعی در میان است.
    /// </summary>
    public enum InstallmentPaymentStatus
    {
        /// <summary>روش انتخاب شده ولی هنوز قطعی نیست (مثلاً منتظر روز حقوق).</summary>
        Selected = 0,

        /// <summary>چک ثبت شده و منتظر تأیید ادمین است.</summary>
        AwaitingAdminApproval = 1,

        /// <summary>پرداخت قطعی شد و قسط تسویه است.</summary>
        Confirmed = 2,

        /// <summary>ادمین چک را رد کرد؛ قسط دوباره پرداخت‌نشده می‌شود.</summary>
        Rejected = 3,

        /// <summary>پرداخت آنلاین ناموفق یا منقضی شد.</summary>
        Failed = 4
    }
}
