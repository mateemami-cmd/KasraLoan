using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class LoanRequestRepository : ILoanRequestRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoanRequestRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LoanRequest request)
        {
            await _context.LoanRequests.AddAsync(request);
        }

        public async Task<LoanRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LoanRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<LoanRequest>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.LoanRequests
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}