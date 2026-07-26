using KasraLoan.Application.Common.Results;
using KasraLoan.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanInstallments
{
    public class GetLoanInstallmentsHandler
    : IRequestHandler<
        GetLoanInstallmentsQuery,
        ApiResponse<List<GetLoanInstallmentsResponse>>>
    {
        private readonly ILoanInstallmentService _loanInstallmentService;

        public GetLoanInstallmentsHandler(
            ILoanInstallmentService loanInstallmentService)
        {
            _loanInstallmentService = loanInstallmentService;
        }

        public async Task<ApiResponse<List<GetLoanInstallmentsResponse>>> Handle(
            GetLoanInstallmentsQuery request,
            CancellationToken cancellationToken)
        {
            var result =
                await _loanInstallmentService
                    .GetLoanInstallmentsAsync(request.LoanId);

            if (!result.IsSuccess)
            {
                return new ApiResponse<List<GetLoanInstallmentsResponse>>
                {
                    IsSuccess = false,
                    Message = result.Message
                };
            }

            return new ApiResponse<List<GetLoanInstallmentsResponse>>
            {
                IsSuccess = true,
                Data = result.Data.Select(x => new GetLoanInstallmentsResponse
                {
                    Id = x.Id,
                    InstallmentNumber = x.InstallmentNumber,
                    Amount = x.Amount,
                    DueDate = x.DueDate,
                    IsPaid = x.IsPaid,
                    PaidAt = x.PaidAt
                }).ToList()
            };
        }
    }
}