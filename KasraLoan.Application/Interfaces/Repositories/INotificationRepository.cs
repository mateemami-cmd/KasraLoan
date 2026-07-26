using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);

        Task<List<Notification>> GetByEmployeeIdAsync(Guid employeeId);

        Task<Notification?> GetByIdAsync(Guid id);

        Task SaveChangesAsync();
    }
}