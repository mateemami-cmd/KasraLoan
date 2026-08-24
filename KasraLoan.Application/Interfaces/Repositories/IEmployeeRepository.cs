using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(Guid id);

        Task<Employee?> GetByUsernameAsync(string username);

        /// <summary>یافتنِ کارمند با ایمیل (حساس‌نبودن به بزرگ/کوچیکی حروف).</summary>
        Task<Employee?> GetByEmailAsync(string email);

        Task<bool> UsernameExistsAsync(string username);

        Task<bool> PersonnelNumberExistsAsync(string personnelNumber);

        Task<Employee?> GetByPersonnelNumberAsync(string personnelNumber);

        Task<List<Employee>> GetAllAsync();

        Task AddAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task SaveChangesAsync();
    }
}