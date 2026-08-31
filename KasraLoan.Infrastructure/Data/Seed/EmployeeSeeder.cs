using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KasraLoan.Infrastructure.Data.Seed
{
    public static class EmployeeSeeder
    {
        // ⚠️ این رمز فقط برای محیط توسعه (Development) است.
        // بلافاصله بعد از اولین ورود باید عوض شود و هرگز نباید در Production استفاده شود.
        private const string DefaultAdminPassword = "Admin@12345";

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
        }

        private static async Task UpsertEmployeeAsync(KasraLoanDbContext context, Employee employee)
        {
            var existingEmployee = await context.Employees.FirstOrDefaultAsync(x => x.PersonnelNumber == employee.PersonnelNumber);

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

            if (string.IsNullOrEmpty(existingEmployee.Username))
                existingEmployee.Username = employee.Username;

            if (string.IsNullOrEmpty(existingEmployee.PasswordHash))
                existingEmployee.PasswordHash = employee.PasswordHash;
        }
    }
}