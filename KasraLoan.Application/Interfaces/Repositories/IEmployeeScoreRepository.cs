using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IEmployeeScoreRepository
    {
        Task<EmployeeScore?> GetByEmployeeIdAsync(Guid employeeId);

        //Task AddAsync(EmployeeScore score);

        //Task SaveChangesAsync();
    }
}