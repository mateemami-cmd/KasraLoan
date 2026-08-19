using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class LoanPermissionRequestRepository : ILoanPermissionRequestRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoanPermissionRequestRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LoanPermissionRequest request)
        {
            await _context.LoanPermissionRequests.AddAsync(request);
        }

        public async Task<LoanPermissionRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LoanPermissionRequests
                .Include(x => x.Employee)
                .Include(x => x.LoanType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<LoanPermissionRequest>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.LoanPermissionRequests
                .Include(x => x.LoanType)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPendingRequestAsync(Guid employeeId)
        {
            return await _context.LoanPermissionRequests
                .AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.Status == LoanPermissionRequestStatus.Pending);
        }

        public async Task<List<LoanPermissionRequest>> GetPagedAsync(int page, int pageSize, LoanPermissionRequestStatus? status, int? loanTypeId = null)
        {
            var query = BuildPagedFilterQuery(status, loanTypeId);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPagedCountAsync(LoanPermissionRequestStatus? status, int? loanTypeId = null)
        {
            return await BuildPagedFilterQuery(status, loanTypeId).CountAsync();
        }

        private IQueryable<LoanPermissionRequest> BuildPagedFilterQuery(LoanPermissionRequestStatus? status, int? loanTypeId = null)
        {
            var query = _context.LoanPermissionRequests
                .Include(x => x.Employee)
                .Include(x => x.LoanType)
                .AsQueryable();

            // «ادمین وام» فقط درخواست‌های مجوزِ نوع وام خودش را می‌بیند.
            if (loanTypeId.HasValue)
            {
                query = query.Where(x => x.LoanTypeId == loanTypeId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            return query;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
