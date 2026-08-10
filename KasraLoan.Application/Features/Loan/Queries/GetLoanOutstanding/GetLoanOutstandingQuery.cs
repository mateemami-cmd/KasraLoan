using KasraLoan.Application.DTOs.Loans;
using MediatR;
using System;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanOutstanding
{
    public class GetLoanOutstandingQuery : IRequest<LoanOutstandingDto>
    {
        public Guid LoanRequestId { get; set; }
    }
}
