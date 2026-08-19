namespace KasraLoan.Application.DTOs.Employee
{
    /// <summary>
    /// تغییر سطح دسترسی یک ادمینِ موجود: ارشد کردن، یا سپردنِ یک نوع وام به او.
    /// </summary>
    public class SetAdminScopeRequestDto
    {
        /// <summary>true یعنی ادمین ارشد (دسترسی کامل)؛ false یعنی ادمین وام.</summary>
        public bool IsSeniorAdmin { get; set; }

        /// <summary>برای ادمین وام لازم است: شناسه‌ی نوع وامی که مدیریت می‌کند.</summary>
        public int? ManagedLoanTypeId { get; set; }
    }
}
