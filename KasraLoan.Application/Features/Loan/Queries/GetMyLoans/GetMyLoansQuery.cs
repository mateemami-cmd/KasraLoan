using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans
{
    public class GetMyLoansQuery : IRequest<List<GetMyLoansResponse>>
    {
    }
}