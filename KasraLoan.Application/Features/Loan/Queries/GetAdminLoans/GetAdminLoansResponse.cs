using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetAdminLoans
{
    public class GetAdminLoansResponse
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        public int RequestedAmount { get; set; }

        public int ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}