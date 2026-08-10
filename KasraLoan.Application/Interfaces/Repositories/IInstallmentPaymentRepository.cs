using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IInstallmentPaymentRepository
    {
        Task AddAsync(InstallmentPayment payment);

        Task<InstallmentPayment?> GetByIdAsync(Guid id);

        /// <summary>رکورد پرداخت را همراه قسط و وامش می‌آورد.</summary>
        Task<InstallmentPayment?> GetByIdWithInstallmentAsync(Guid id);

        /// <summary>نشستِ پرداخت آنلاین را با شناسه‌ی درگاه پیدا می‌کند.</summary>
        Task<InstallmentPayment?> GetByAuthorityAsync(Guid authority);

        /// <summary>آخرین تلاش پرداختِ یک قسط که هنوز رد یا ناموفق نشده.</summary>
        Task<InstallmentPayment?> GetActiveForInstallmentAsync(Guid installmentId);

        /// <summary>صف چک‌های منتظر تأیید ادمین.</summary>
        Task<List<InstallmentPayment>> GetPendingChequesAsync();

        Task SaveChangesAsync();
    }
}
