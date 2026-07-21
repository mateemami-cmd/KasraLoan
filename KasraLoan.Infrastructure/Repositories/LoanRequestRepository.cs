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
                .Include(x => x.Employee)
                .Include(x => x.LoanType)
                .Include(x => x.LoanInstallments)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<LoanRequest>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.LoanRequests
                .Include(x => x.LoanType)
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<LoanRequest?> GetPendingLoanByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.LoanRequests
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.Status == LoanStatus.Pending);
        }

        public async Task<List<LoanRequest>> GetAllAsync()
        {
            return await _context.LoanRequests.ToListAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _context.LoanRequests
                .CountAsync(x => x.Status == LoanStatus.Pending);
        }

        public async Task<int> GetApprovedCountAsync()
        {
            return await _context.LoanRequests
                .CountAsync(x => x.Status == LoanStatus.Approved);
        }

        public async Task<int> GetRejectedCountAsync()
        {
            return await _context.LoanRequests
                .CountAsync(x => x.Status == LoanStatus.Rejected);
        }

        public async Task<decimal> GetTotalRequestedAmountAsync()
        {
            return await _context.LoanRequests
                .SumAsync(x => (decimal)x.RequestedAmount);
        }

        public async Task<decimal> GetTotalApprovedAmountAsync()
        {
            return await _context.LoanRequests
                .SumAsync(x => (decimal)x.ApprovedAmount);
        }

        public async Task<List<LoanRequest>> GetPagedAsync(int page, int pageSize, LoanStatus? status, string? search)
        {
            var query = _context.LoanRequests
                .Include(x => x.Employee)
                .Include(x => x.LoanType)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Employee.FirstName.Contains(search) ||
                    x.Employee.LastName.Contains(search) ||
                    x.Employee.PersonnelNumber.Contains(search));
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}