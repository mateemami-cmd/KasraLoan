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
        public static async Task SeedAsync(KasraLoanDbContext context)
        {

            var admin = new Employee
            {
                Id =
                //Guid.Parse("ac56f19c-8b8d-41ff-b47e-93d237fd24c4"),
                SeedIds.AdminAli,
                FirstName = "Ali",
                LastName = "Ahmadi",
                PersonnelNumber = "1001",
                HireDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                Role = UserRole.Admin
            };

            //var employees = new List<Employee>
            //{
            //    admin,

            //    new Employee
            //    {
            //        Id = SeedIds.Reza,
            //        FirstName = "Reza",
            //        LastName = "Mohammadi",
            //        PersonnelNumber = "1002",
            //        HireDate = new DateTime(2021, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Sara,
            //        FirstName = "Sara",
            //        LastName = "Hosseini",
            //        PersonnelNumber = "1003",
            //        HireDate = new DateTime(2022, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            //        MarriageDate = DateTime.UtcNow.Date.AddMonths(-3),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Amir,
            //        FirstName = "Amir",
            //        LastName = "Karimi",
            //        PersonnelNumber = "1004",
            //        HireDate = new DateTime(2019, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Nima,
            //        FirstName = "Nima",
            //        LastName = "Ghasemi",
            //        PersonnelNumber = "1005",
            //        HireDate = new DateTime(2020, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Zahra,
            //        FirstName = "Zahra",
            //        LastName = "Ahmadi",
            //        PersonnelNumber = "1006",
            //        HireDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Mehdi,
            //        FirstName = "Mehdi",
            //        LastName = "Azizi",
            //        PersonnelNumber = "1007",
            //        HireDate = new DateTime(2018, 4, 5, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = false,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Hanieh,
            //        FirstName = "Hanieh",
            //        LastName = "Moradi",
            //        PersonnelNumber = "1008",
            //        HireDate = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Sina,
            //        FirstName = "Sina",
            //        LastName = "Ebrahimi",
            //        PersonnelNumber = "1009",
            //        HireDate = new DateTime(2020, 11, 11, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    },

            //    new Employee
            //    {
            //        Id = SeedIds.Parisa,
            //        FirstName = "Parisa",
            //        LastName = "Rahimi",
            //        PersonnelNumber = "1010",
            //        HireDate = new DateTime(2021, 9, 9, 0, 0, 0, DateTimeKind.Utc),
            //        IsActive = true,
            //        Role = UserRole.Employee
            //    }
            //};

            await UpsertEmployeeAsync(context, admin);

            await context.SaveChangesAsync();

            //foreach (var employee in employees)
            //{
            //    await UpsertEmployeeAsync(context, employee);
            //}

            //await context.SaveChangesAsync();
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
        }
    }
}