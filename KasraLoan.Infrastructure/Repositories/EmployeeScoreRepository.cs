using KasraLoan.Application.Interfaces.Repositories;
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

        public async Task<int> GetScoreByEmployeeIdAsync(int employeeId)
        {
            // فعلاً فرض: جدول EmployeeScores داری
            var score = await _context.EmployeeScores
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Score)
                .FirstOrDefaultAsync();

            return score;
        }
    }
}