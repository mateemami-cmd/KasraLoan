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

        Task SaveChangesAsync();

        Task<List<LoanInstallment>> GetByLoanIdAsync(Guid loanId);
    }
}