using KasraLoan.Application.DTOs.Loans;
using System;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// مانده‌ی وام و مطالبه‌ی تسویه‌ی یکجا.
    ///
    /// «مانده» همیشه از روی اقساط پرداخت‌نشده حساب می‌شود، نه از یک فیلد ذخیره‌شده،
    /// تا هیچ‌وقت با واقعیت اقساط اختلاف پیدا نکند.
    /// </summary>
    public interface ILoanSettlementService
    {
        /// <summary>جمع اقساط پرداخت‌نشده‌ی یک وام.</summary>
        Task<LoanOutstandingDto> GetOutstandingAsync(Guid loanRequestId);

        /// <summary>
        /// کل مانده‌ی وام‌های بازِ کارمند را یکجا مطالبه می‌کند و مهلت تعیین می‌کند.
        /// اگر وام بازی نداشته باشد، هیچ کاری نمی‌کند و null برمی‌گرداند.
        /// </summary>
        Task<LoanSettlementDemandDto?> DemandSettlementForEmployeeAsync(
            Guid employeeId,
            string reason);
    }
}
