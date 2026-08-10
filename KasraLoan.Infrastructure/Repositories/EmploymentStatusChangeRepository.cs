using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class EmploymentStatusChangeRepository : IEmploymentStatusChangeRepository
    {
        private readonly KasraLoanDbContext _context;

        public EmploymentStatusChangeRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EmploymentStatusChange change)
        {
            await _context.EmploymentStatusChanges.AddAsync(change);
        }

        public async Task<List<EmploymentStatusChange>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.EmploymentStatusChanges
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
