namespace KasraLoan.Domain.Enums
{
    /// <summary>
    /// وضعیت اشتغال کارمند. عمداً از <c>Employee.IsActive</c> جداست:
    ///
    ///   IsActive         → حساب کاربری: می‌تواند وارد سیستم شود یا نه
    ///   EmploymentStatus → واقعیت شغلی: مشغول به کار هست یا نه
    ///
    /// دلیل جدا بودنشان: کارمندی که از شرکت می‌رود ولی وام تسویه‌نشده دارد باید
    /// همچنان بتواند وارد شود، اقساطش را ببیند و پرداخت کند. اگر یک فیلد بودند،
    /// «رفتنِ» او دسترسی‌اش به بدهی خودش را هم قطع می‌کرد.
    /// </summary>
    public enum EmploymentStatus
    {
        /// <summary>مشغول به کار.</summary>
        Active = 0,

        /// <summary>دیگر در شرکت کار نمی‌کند.</summary>
        Terminated = 1
    }
}
