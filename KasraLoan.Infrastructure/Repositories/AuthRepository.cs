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
    public class AuthRepository : IAuthRepository
    {
        private readonly KasraLoanDbContext _context;

        public AuthRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetEmployeeByLoginTokenAsync(string token)
        {
            var loginToken = await _context.EmployeeLoginTokens
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Token == token && x.IsActive);

            return loginToken?.Employee;
        }
    }
}