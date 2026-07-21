using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans.GetAllLoans
{
    public class GetAllLoansQuery : IRequest<List<GetAllLoansResponse>>
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public LoanStatus? Status { get; set; }

        public string? Search { get; set; }
    }
}