using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(
            Guid? employeeId,
            Guid? loanRequestId,
            string action,
            string description);
    }
}