using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoanDocumentRepository
    {
        Task AddAsync(LoanDocument document);

        Task<List<LoanDocument>> GetByLoanIdAsync(Guid loanRequestId);

        Task<bool> ExistsAsync(Guid loanRequestId);

        Task SaveChangesAsync();
    }
}