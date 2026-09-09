using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class LoanFundRepository : ILoanFundRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoanFundRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<LoanFund?> GetAsync()
        {
            // صندوق فقط یک ردیف دارد؛ همان اولین (و تنها) ردیف را برمی‌گردانیم.
            return await _context.LoanFunds.FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
