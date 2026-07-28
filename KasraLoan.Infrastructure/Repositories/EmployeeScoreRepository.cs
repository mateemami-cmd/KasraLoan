using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class EmployeeScoreRepository : IEmployeeScoreRepository
    {
        private readonly KasraLoanDbContext _context;

        public EmployeeScoreRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeScore?> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.EmployeeScores
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }

        public async Task AddAsync(EmployeeScore score)
        {
            await _context.EmployeeScores.AddAsync(score);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}