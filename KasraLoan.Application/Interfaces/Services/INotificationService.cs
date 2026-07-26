using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendAsync(
        Guid employeeId,
        string title,
        string message);
    }
}