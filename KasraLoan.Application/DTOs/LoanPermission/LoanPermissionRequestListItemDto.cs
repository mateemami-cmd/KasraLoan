using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.LoanPermission
{
    public class LoanPermissionRequestListItemDto
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeUsername { get; set; } = string.Empty;

        public int LoanTypeId { get; set; }

        public string LoanTypeName { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? AdminResponse { get; set; }
    }
}
