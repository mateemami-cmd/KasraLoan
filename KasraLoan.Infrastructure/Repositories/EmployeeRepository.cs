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
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly KasraLoanDbContext _context;

        public EmployeeRepository(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            // JobPosition عمداً همیشه Include می‌شود: محاسبه‌ی حقوق مؤثر به آن نیاز دارد
            // و اگر بارگذاری نشده باشد، حقوق بی‌سروصدا صفر حساب می‌شود. یک join روی
            // جدولی چند‌رکوردی ارزشش را دارد که این دام وجود نداشته باشد.
            return await _context.Employees
                .Include(x => x.JobPosition)
                .Include(x => x.ManagedLoanType)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Employee?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.Employees
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Employees
                .AnyAsync(x => x.Username == username);
        }

        public async Task<bool> PersonnelNumberExistsAsync(string personnelNumber)
        {
            return await _context.Employees
                .AnyAsync(x => x.PersonnelNumber == personnelNumber);
        }

        public async Task<Employee?> GetByPersonnelNumberAsync(string personnelNumber)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(x => x.PersonnelNumber == personnelNumber);
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(x => x.JobPosition)
                .Include(x => x.ManagedLoanType)
                .ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}