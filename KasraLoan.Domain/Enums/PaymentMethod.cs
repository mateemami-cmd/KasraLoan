namespace KasraLoan.Domain.Enums
{
    /// <summary>روشی که کارمند برای پرداخت قسط انتخاب می‌کند.</summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// کسر از حقوق. پیش‌فرض است: اگر کارمند تا پایان پنجره‌ی انتخاب چیزی
        /// انتخاب نکند، همین اعمال می‌شود.
        /// </summary>
        PayrollDeduction = 0,

        /// <summary>پرداخت آنلاین از طریق درگاه.</summary>
        OnlineGateway = 1,

        /// <summary>چک؛ کارمند تصویر چک را بارگذاری می‌کند و ادمین تأیید می‌کند.</summary>
        Cheque = 2
    }
}
