using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetAdminDashboard
{
    public class GetAdminDashboardHandler : IRequestHandler<GetAdminDashboardQuery, GetAdminDashboardResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;

        public GetAdminDashboardHandler(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<GetAdminDashboardResponse> Handle(
            GetAdminDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var allLoans = await _loanRequestRepository.GetAllAsync();

            return new GetAdminDashboardResponse
            {
                TotalLoans = allLoans.Count,
                PendingLoans = await _loanRequestRepository.GetPendingCountAsync(),
                ApprovedLoans = await _loanRequestRepository.GetApprovedCountAsync(),
                RejectedLoans = await _loanRequestRepository.GetRejectedCountAsync(),
                TotalRequestedAmount = await _loanRequestRepository.GetTotalRequestedAmountAsync(),
                TotalApprovedAmount = await _loanRequestRepository.GetTotalApprovedAmountAsync()
            };
        }
    }
}