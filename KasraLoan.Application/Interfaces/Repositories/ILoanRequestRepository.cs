using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoanRequestRepository
    {
        Task AddAsync(LoanRequest request);

        Task<LoanRequest?> GetByIdAsync(Guid id);

        Task<List<LoanRequest>> GetByEmployeeIdAsync(Guid employeeId);

        Task<LoanRequest?> GetPendingLoanByEmployeeIdAsync(Guid employeeId);

        Task<List<LoanRequest>> GetAllAsync();

        Task<int> GetPendingCountAsync();

        Task<int> GetApprovedCountAsync();

        Task<int> GetRejectedCountAsync();

        Task<decimal> GetTotalRequestedAmountAsync();

        Task<decimal> GetTotalApprovedAmountAsync();

        Task<List<LoanRequest>> GetPagedAsync(int page, int pageSize);

        Task SaveChangesAsync();
    }
}