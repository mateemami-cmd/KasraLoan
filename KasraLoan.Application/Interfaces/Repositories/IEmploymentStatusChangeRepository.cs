using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IEmploymentStatusChangeRepository
    {
        Task AddAsync(EmploymentStatusChange change);

        /// <summary>تاریخچه‌ی یک کارمند، از جدیدترین به قدیمی‌ترین.</summary>
        Task<List<EmploymentStatusChange>> GetByEmployeeIdAsync(Guid employeeId);

        Task SaveChangesAsync();
    }
}
