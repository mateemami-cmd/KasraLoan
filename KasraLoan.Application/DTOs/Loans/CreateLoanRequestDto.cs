using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Loans
{
    public class CreateLoanRequestDto
    {
        public int EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        public int RequestedAmount { get; set; }

        public int InstallmentCount { get; set; }
    }
}