using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KasraLoan.Infrastructure.Data
{
    public class KasraLoanDbContext : DbContext
    {
        public KasraLoanDbContext(DbContextOptions<KasraLoanDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeLoginToken> EmployeeLoginTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<LoanType> LoanTypes { get; set; }
        public DbSet<LoanRule> LoanRules { get; set; }
        public DbSet<LoanRequest> LoanRequests { get; set; }
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<EmployeeScore> EmployeeScores { get; set; }
    }
}