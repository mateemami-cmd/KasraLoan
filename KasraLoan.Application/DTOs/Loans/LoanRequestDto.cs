using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Loans
{
    public class LoanRequestDto
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        public int RequestedAmount { get; set; }

        public int ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public LoanStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TotalPayableAmount { get; set; }

        public decimal MonthlyPaymentAmount { get; set; }
    }
}