using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Entities;
using System;

namespace KasraLoan.Application.Services
{
    /// <inheritdoc cref="IEmployeeSalaryService"/>
    public class EmployeeSalaryService : IEmployeeSalaryService
    {
        // یک‌سوم حقوق. عمداً اینجا به‌صورت ثابت است تا فعلاً یک مرجع داشته باشد؛
        // وقتی لازم شد پلکانی یا قابل تنظیم از پنل شود، فقط همین یک نقطه عوض می‌شود.
        public decimal MaxInstallmentToSalaryPercent => 33.33m;

        public long GetEffectiveMonthlySalary(Employee employee)
        {
            if (employee is null)
                return 0;

            if (employee.MonthlySalary.HasValue)
                return employee.MonthlySalary.Value;

            return employee.JobPosition?.BaseSalary ?? 0;
        }

        public decimal GetMaxMonthlyInstallment(Employee employee)
        {
            var salary = GetEffectiveMonthlySalary(employee);

            if (salary <= 0)
                return 0;

            return Math.Floor(salary * (MaxInstallmentToSalaryPercent / 100m));
        }
    }
}
