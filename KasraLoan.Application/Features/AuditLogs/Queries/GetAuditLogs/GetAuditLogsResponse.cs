using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public class GetAuditLogsResponse
    {
        public Guid Id { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? LoanRequestId { get; set; }

        public string Action { get; set; } = default!;

        public string Description { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}