using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestResponse
    {
        public Guid LoanRequestId { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}