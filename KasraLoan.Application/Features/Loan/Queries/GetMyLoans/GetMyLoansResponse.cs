using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans
{
    public class GetMyLoansResponse
    {
        public Guid Id { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal RequestedAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}