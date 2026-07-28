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

        Task<bool> UsernameExistsAsync(string username);

        Task<bool> PersonnelNumberExistsAsync(string personnelNumber);

        Task<List<Employee>> GetAllAsync();

        Task AddAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task DeleteAsync(Employee employee);

        Task SaveChangesAsync();
    }
}