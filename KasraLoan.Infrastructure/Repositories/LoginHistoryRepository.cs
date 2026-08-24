using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasraLoan.Infrastructure.Repositories
{
    public class LoginHistoryRepository : ILoginHistoryRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoginHistoryRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LoginHistory entry)
        {
            await _context.LoginHistories.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LoginHistory>> GetRecentByEmployeeAsync(Guid employeeId, int count)
        {
            return await _context.LoginHistories
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AttemptedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
