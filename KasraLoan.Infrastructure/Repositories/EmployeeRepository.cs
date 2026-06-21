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
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee?> GetByLoginTokenAsync(string token)
        {
            var loginToken = await _context.EmployeeLoginTokens
                .FirstOrDefaultAsync(x => x.Token == token && x.IsActive);

            if (loginToken == null)
                return null;

            return await _context.Employees
                .FirstOrDefaultAsync(x => x.Id == loginToken.EmployeeId);
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
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

        public Task DeleteAsync(Employee employee)
        {
            _context.Employees.Remove(employee);
            return Task.CompletedTask;
        }
    }
}