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
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LoanType> LoanTypes { get; set; }
        public DbSet<LoanRule> LoanRules { get; set; }
        public DbSet<LoanRequest> LoanRequests { get; set; }
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<EmployeeScore> EmployeeScores { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LoanPermissionRequest> LoanPermissionRequests { get; set; }
        public DbSet<JobPosition> JobPositions { get; set; }
        public DbSet<EmploymentStatusChange> EmploymentStatusChanges { get; set; }
        public DbSet<InstallmentPayment> InstallmentPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.Entity<JobPosition>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(x => x.Title)
                    .IsUnique();

                entity.Property(x => x.IsActive)
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<Employee>()
                .HasOne(x => x.JobPosition)
                .WithMany(x => x.Employees)
                .HasForeignKey(x => x.JobPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InstallmentPayment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ChequeNumber).HasMaxLength(50);
                entity.Property(x => x.ChequeBankName).HasMaxLength(100);
                entity.Property(x => x.ChequeImageUrl).HasMaxLength(500);
                entity.Property(x => x.GatewayRefId).HasMaxLength(100);
                entity.Property(x => x.RejectReason).HasMaxLength(500);

                // صف تأیید چک و «آخرین تلاش این قسط» هر دو از این ایندکس می‌آیند.
                entity.HasIndex(x => new { x.LoanInstallmentId, x.CreatedAt });

                entity.HasIndex(x => x.Status);

                // نشستِ درگاه با همین شناسه پیدا می‌شود، پس باید یکتا باشد.
                entity.HasIndex(x => x.GatewayAuthority)
                    .IsUnique()
                    .HasFilter("\"GatewayAuthority\" IS NOT NULL");

                entity.HasOne(x => x.LoanInstallment)
                    .WithMany(x => x.Payments)
                    .HasForeignKey(x => x.LoanInstallmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmploymentStatusChange>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Reason)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.ChangedAt)
                    .IsRequired();

                entity.HasIndex(x => new { x.EmployeeId, x.ChangedAt });

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

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

            // جزئیات مخصوص هر نوع وام در یک ستون jsonb؛ در کد کلاس است، در
            // دیتابیس یک ستون. با اضافه شدن انواع دیگر، فقط زیرشاخه اضافه می‌شود
            // و مایگریشن جدید لازم نیست.
            modelBuilder.Entity<LoanRequest>()
                .OwnsOne(x => x.Details, details =>
                {
                    details.ToJson();
                    details.OwnsOne(d => d.Travel);
                });

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

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Message)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.IsRead)
                    .HasDefaultValue(false);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LoanPermissionRequest>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Reason)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.AdminResponse)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasOne(x => x.Employee)
                    .WithMany()
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.LoanType)
                    .WithMany()
                    .HasForeignKey(x => x.LoanTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}