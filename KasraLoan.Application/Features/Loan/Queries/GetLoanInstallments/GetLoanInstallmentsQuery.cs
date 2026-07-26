using KasraLoan.Application.Common.Results;
using MediatR;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanInstallments;

public class GetLoanInstallmentsQuery
    : IRequest<ApiResponse<List<GetLoanInstallmentsResponse>>>
{
    public Guid LoanId { get; set; }

    public GetLoanInstallmentsQuery(Guid loanId)
    {
        LoanId = loanId;
    }
}