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
    }
}