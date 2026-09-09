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
        private readonly ILoanFundRepository _loanFundRepository;

        public GetAdminDashboardHandler(
            ILoanRequestRepository loanRequestRepository,
            ILoanFundRepository loanFundRepository)
        {
            _loanRequestRepository = loanRequestRepository;
            _loanFundRepository = loanFundRepository;
        }

        public async Task<GetAdminDashboardResponse> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
        {
            var allLoans = await _loanRequestRepository.GetAllAsync();
            var fund = await _loanFundRepository.GetAsync();

            return new GetAdminDashboardResponse
            {
                TotalLoans = allLoans.Count,
                PendingLoans = await _loanRequestRepository.GetPendingCountAsync(),
                ApprovedLoans = await _loanRequestRepository.GetApprovedCountAsync(),
                RejectedLoans = await _loanRequestRepository.GetRejectedCountAsync(),
                TotalRequestedAmount = await _loanRequestRepository.GetTotalRequestedAmountAsync(),
                TotalApprovedAmount = await _loanRequestRepository.GetTotalApprovedAmountAsync(),
                FundBalance = fund?.Balance ?? 0
            };
        }
    }
}