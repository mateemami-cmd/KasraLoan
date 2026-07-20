using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanById
{
    public class GetLoanByIdResponse
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string LoanType { get; set; } = string.Empty;

        public decimal RequestedAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public decimal MonthlyPaymentAmount { get; set; }

        public decimal TotalPayableAmount { get; set; }
    }
}