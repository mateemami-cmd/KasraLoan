using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? LoanRequestId { get; set; }

        public string Action { get; set; } = default!;

        public string Description { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public Employee? Employee { get; set; }

        public LoanRequest? LoanRequest { get; set; }
    }
}