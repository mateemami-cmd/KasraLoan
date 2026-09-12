using System;
using System.Linq;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KasraLoan.Infrastructure.Data.Seed
{
    public static class EmployeeSeeder
    {
        // ⚠️ این رمزها فقط برای محیط توسعه/دمو هستند و نباید در Production استفاده شوند.
        private const string DefaultAdminPassword = "Admin@12345";
        private const string GoodEmployeePassword = "Sara@12345";
        private const string LowEmployeePassword = "Reza@12345";

        public static async Task SeedAsync(KasraLoanDbContext context)
        {
            var admin = new Employee
            {
                Id = SeedIds.AdminAli,
                FirstName = "علی",
                LastName = "احمدی",
                PersonnelNumber = "1001",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword, workFactor: 12),
                HireDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                Role = UserRole.Admin,
                IsSeniorAdmin = true
            };

            await UpsertEmployeeAsync(context, admin);
            await context.SaveChangesAsync();

            // سمت شغلی برای کارمندهای دمو (برای محاسبه‌ی حقوق و سقف قسط لازم است).
            var position = await context.JobPositions
                .FirstOrDefaultAsync(x => x.Title == "توسعه‌دهنده بک‌اند");

            // کارمندِ دمو با امتیازِ خوب: استخدامِ قدیمی → امتیازِ بالا و سابقه‌ی > ۱ سال
            // (هم برای وام‌های عادی و هم وام ازدواج واجد شرایط است) → سناریوی درخواستِ موفق.
            var goodEmployee = new Employee
            {
                Id = SeedIds.EmployeeSaraGoodScore,
                FirstName = "سارا",
                LastName = "کریمی",
                PersonnelNumber = "1002",
                Username = "sara",
                NationalId = "1234567891",
                PhoneNumber = "09120000002",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(GoodEmployeePassword, workFactor: 12),
                HireDate = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                Role = UserRole.Employee,
                EmploymentStatus = EmploymentStatus.Active,
                JobPositionId = position?.Id
            };

            // کارمندِ دمو با امتیازِ پایین: استخدامِ اخیر (۲ ماه پیش) → امتیاز ~۲۰۰
            // که کمتر از حداقلِ ۶۰۰ است → درخواستِ وام رد می‌شود. تاریخ نسبی است تا
            // با گذر زمان همچنان «پایین» بماند.
            var lowEmployee = new Employee
            {
                Id = SeedIds.EmployeeRezaLowScore,
                FirstName = "رضا",
                LastName = "محمدی",
                PersonnelNumber = "1003",
                Username = "reza",
                NationalId = "1234567892",
                PhoneNumber = "09120000003",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(LowEmployeePassword, workFactor: 12),
                HireDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddMonths(-2), DateTimeKind.Utc),
                IsActive = true,
                Role = UserRole.Employee,
                EmploymentStatus = EmploymentStatus.Active,
                JobPositionId = position?.Id
            };

            await UpsertEmployeeAsync(context, goodEmployee);
            await UpsertEmployeeAsync(context, lowEmployee);
            await context.SaveChangesAsync();

            // کارمند برای ثبت درخواستِ وام حتماً باید ردیفِ EmployeeScore داشته باشد
            // (وگرنه هندلرِ ساختِ وام خطا می‌دهد). امتیاز خودکار از روی سابقه حساب می‌شود.
            await EnsureScoreAsync(context, goodEmployee.PersonnelNumber);
            await EnsureScoreAsync(context, lowEmployee.PersonnelNumber);
            await context.SaveChangesAsync();
        }

        private static async Task EnsureScoreAsync(KasraLoanDbContext context, string personnelNumber)
        {
            var employee = await context.Employees
                .FirstOrDefaultAsync(x => x.PersonnelNumber == personnelNumber);

            if (employee == null)
                return;

            var hasScore = await context.EmployeeScores.AnyAsync(x => x.EmployeeId == employee.Id);

            if (!hasScore)
            {
                context.EmployeeScores.Add(new EmployeeScore
                {
                    EmployeeId = employee.Id,
                    ManualOverrideScore = null,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private static async Task UpsertEmployeeAsync(KasraLoanDbContext context, Employee employee)
        {
            var existingEmployee = await context.Employees
                .FirstOrDefaultAsync(x => x.PersonnelNumber == employee.PersonnelNumber);

            if (existingEmployee == null)
            {
                context.Employees.Add(employee);
                return;
            }

            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.HireDate = employee.HireDate;
            existingEmployee.MarriageDate = employee.MarriageDate;
            existingEmployee.IsActive = employee.IsActive;
            existingEmployee.Role = employee.Role;
            existingEmployee.IsSeniorAdmin = employee.IsSeniorAdmin;
            existingEmployee.EmploymentStatus = employee.EmploymentStatus;
            existingEmployee.JobPositionId = employee.JobPositionId;

            if (string.IsNullOrEmpty(existingEmployee.Username))
                existingEmployee.Username = employee.Username;

            if (string.IsNullOrEmpty(existingEmployee.PasswordHash))
                existingEmployee.PasswordHash = employee.PasswordHash;

            // این‌ها فقط وقتی خالی‌اند پر می‌شوند تا داده‌ی واقعیِ ادمین/کارمند بازنویسی نشود.
            if (string.IsNullOrEmpty(existingEmployee.NationalId))
                existingEmployee.NationalId = employee.NationalId;

            if (string.IsNullOrEmpty(existingEmployee.PhoneNumber))
                existingEmployee.PhoneNumber = employee.PhoneNumber;
        }
    }
}
