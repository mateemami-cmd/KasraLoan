using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class InstallmentPaymentRepository : IInstallmentPaymentRepository
    {
        private readonly KasraLoanDbContext _context;

        public InstallmentPaymentRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InstallmentPayment payment)
        {
            await _context.InstallmentPayments.AddAsync(payment);
        }

        public async Task<InstallmentPayment?> GetByIdAsync(Guid id)
        {
            return await _context.InstallmentPayments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<InstallmentPayment?> GetByIdWithInstallmentAsync(Guid id)
        {
            return await _context.InstallmentPayments
                .Include(x => x.LoanInstallment)
                    .ThenInclude(i => i.LoanRequest)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<InstallmentPayment?> GetByAuthorityAsync(Guid authority)
        {
            return await _context.InstallmentPayments
                .Include(x => x.LoanInstallment)
                    .ThenInclude(i => i.LoanRequest)
                .FirstOrDefaultAsync(x => x.GatewayAuthority == authority);
        }

        public async Task<InstallmentPayment?> GetActiveForInstallmentAsync(Guid installmentId)
        {
            // تلاش‌های ردشده و ناموفق به حساب نمی‌آیند: کارمند بعد از آن‌ها باید
            // بتواند دوباره روش انتخاب کند.
            return await _context.InstallmentPayments
                .Where(x =>
                    x.LoanInstallmentId == installmentId &&
                    x.Status != InstallmentPaymentStatus.Rejected &&
                    x.Status != InstallmentPaymentStatus.Failed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<InstallmentPayment>> GetPendingChequesAsync()
        {
            return await _context.InstallmentPayments
                .Include(x => x.Employee)
                .Include(x => x.LoanInstallment)
                    .ThenInclude(i => i.LoanRequest)
                        .ThenInclude(r => r.LoanType)
                .Where(x => x.Status == InstallmentPaymentStatus.AwaitingAdminApproval)
                // قدیمی‌ترین اول: نزدیک‌ترین به قطعی‌شدن لیست حقوق فوری‌ترین است.
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
