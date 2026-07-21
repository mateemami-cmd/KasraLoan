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
        public DbSet<LoanDocument> LoanDocuments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmployeeScore>()
                .HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId);

            modelBuilder.Entity<LoanRequest>()
                .HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId);

            modelBuilder.Entity<LoanRequest>()
                .HasOne(x => x.LoanType)
                .WithMany(x => x.LoanRequests)
                .HasForeignKey(x => x.LoanTypeId);

            modelBuilder.Entity<LoanDocument>()
                .HasOne(x => x.LoanRequest)
                .WithMany(x => x.LoanDocuments)
                .HasForeignKey(x => x.LoanRequestId);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Action)
                    .HasMaxLength(100);

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.LoanRequest)
                    .WithMany()
                    .HasForeignKey(x => x.LoanRequestId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}