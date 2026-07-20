using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanById
{
    public class GetLoanByIdQuery : IRequest<GetLoanByIdResponse>
    {
        public Guid LoanId { get; set; }
    }
}