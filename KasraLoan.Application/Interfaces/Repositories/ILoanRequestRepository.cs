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

        Task<LoanRequest?> GetByIdAsync(int id);

        Task<List<LoanRequest>> GetByEmployeeIdAsync(int employeeId);

        Task SaveChangesAsync();
    }
}