using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanInstallments
{
    public class GetLoanInstallmentsResponse
    {
        public Guid Id { get; set; }

        public int InstallmentNumber { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}