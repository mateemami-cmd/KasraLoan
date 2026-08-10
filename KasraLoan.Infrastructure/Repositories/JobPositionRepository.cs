using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class JobPositionRepository : IJobPositionRepository
    {
        private readonly KasraLoanDbContext _context;

        public JobPositionRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<JobPosition?> GetByIdAsync(int id)
        {
            return await _context.JobPositions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<JobPosition>> GetAllAsync(bool activeOnly)
        {
            var query = _context.JobPositions.AsQueryable();

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderByDescending(x => x.BaseSalary)
                .ToListAsync();
        }

        public async Task<bool> TitleExistsAsync(string title, int? excludeId = null)
        {
            var query = _context.JobPositions.Where(x => x.Title == title);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> HasEmployeesAsync(int jobPositionId)
        {
            return await _context.Employees
                .AnyAsync(x => x.JobPositionId == jobPositionId);
        }

        public async Task<Dictionary<int, int>> GetEmployeeCountsAsync()
        {
            return await _context.Employees
                .Where(x => x.JobPositionId != null)
                .GroupBy(x => x.JobPositionId!.Value)
                .Select(g => new { JobPositionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.JobPositionId, x => x.Count);
        }

        public async Task AddAsync(JobPosition jobPosition)
        {
            await _context.JobPositions.AddAsync(jobPosition);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
