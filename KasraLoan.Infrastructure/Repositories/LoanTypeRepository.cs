using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Repositories
{
    public class LoanTypeRepository : ILoanTypeRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoanTypeRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<LoanType?> GetByIdAsync(int id)
        {
            return await _context.LoanTypes
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}