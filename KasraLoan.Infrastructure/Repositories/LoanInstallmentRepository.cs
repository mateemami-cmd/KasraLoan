using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class LoanInstallmentRepository : ILoanInstallmentRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoanInstallmentRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<LoanInstallment> installments)
        {
            await _context.LoanInstallments.AddRangeAsync(installments);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}