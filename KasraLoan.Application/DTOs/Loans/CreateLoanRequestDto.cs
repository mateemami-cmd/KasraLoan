using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Loans
{
    public class CreateLoanRequestDto
    {
        public int LoanTypeId { get; set; }

        public long RequestedAmount { get; set; }

        public int InstallmentCount { get; set; }
    }
}