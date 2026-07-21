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
    public class LoanDocumentRepository : ILoanDocumentRepository
    {
        private readonly KasraLoanDbContext _context;

        public LoanDocumentRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LoanDocument document)
        {
            await _context.LoanDocuments.AddAsync(document);
        }

        public async Task<List<LoanDocument>> GetByLoanIdAsync(Guid loanRequestId)
        {
            return await _context.LoanDocuments
                .Where(x => x.LoanRequestId == loanRequestId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid loanRequestId)
        {
            return await _context.LoanDocuments
                .AnyAsync(x => x.LoanRequestId == loanRequestId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}