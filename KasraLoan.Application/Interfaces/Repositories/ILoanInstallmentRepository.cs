using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoanInstallmentRepository
    {
        Task AddRangeAsync(IEnumerable<LoanInstallment> installments);

        Task<LoanInstallment?> GetByIdAsync(Guid installmentId);

        Task<LoanInstallment?> GetByIdWithLoanAsync(Guid installmentId);

        Task<List<LoanInstallment>> GetByLoanIdAsync(Guid loanId);

        Task<bool> AreAllInstallmentsPaidAsync(Guid loanRequestId);

        /// <summary>
        /// آیا به‌جز این قسط، قسط پرداخت‌نشده‌ی دیگری در وام مانده است.
        ///
        /// عمداً قسطِ در حال پرداخت را کنار می‌گذارد: در لحظه‌ی صدا زدن، هنوز
        /// SaveChanges انجام نشده و در دیتابیس پرداخت‌نشده است. شمردن روی
        /// navigation property هم جواب نمی‌دهد، چون EF فقط همان یک قسطِ لود‌شده
        /// را داخلش می‌گذارد و «همه پرداخت شده‌اند» بی‌دلیل درست درمی‌آید.
        /// </summary>
        Task<bool> HasOtherUnpaidInstallmentsAsync(Guid loanRequestId, Guid excludingInstallmentId);

        Task SaveChangesAsync();
    }
}