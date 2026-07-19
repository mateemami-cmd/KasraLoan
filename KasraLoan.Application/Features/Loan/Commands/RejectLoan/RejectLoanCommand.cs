using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.RejectLoan
{
    public class RejectLoanCommand : IRequest<RejectLoanResponse>
    {
        public Guid LoanRequestId { get; set; }
    }
}