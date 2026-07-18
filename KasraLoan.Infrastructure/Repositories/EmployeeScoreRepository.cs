using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KasraLoan.Infrastructure.Repositories
{
    public class EmployeeScoreRepository : IEmployeeScoreRepository
    {
        private readonly KasraLoanDbContext _context;

        public EmployeeScoreRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        //public async Task<int> GetScoreByEmployeeIdAsync(Guid employeeId)
        //{
        //    // فعلاً فرض: جدول EmployeeScores داری
        //    var score = await _context.EmployeeScores
        //        .Where(x => x.EmployeeId == employeeId)
        //        .OrderByDescending(x => x.Id)
        //        .Select(x => x.Score)
        //        .FirstOrDefaultAsync();

        //    return score;
        //}

        public async Task<EmployeeScore?> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.EmployeeScores
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }
    }
}