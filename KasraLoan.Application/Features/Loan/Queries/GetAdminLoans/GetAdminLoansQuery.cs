using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetAdminLoans
{
    public class GetAdminLoansQuery : IRequest<List<GetAdminLoansResponse>>
    {
        public LoanStatus? Status { get; set; }
    }
}