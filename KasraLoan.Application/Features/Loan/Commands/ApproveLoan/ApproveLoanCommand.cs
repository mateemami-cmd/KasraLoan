using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.ApproveLoan
{
    public class ApproveLoanCommand : IRequest<ApproveLoanResponse>
    {
        public Guid LoanRequestId { get; set; }
    }
}