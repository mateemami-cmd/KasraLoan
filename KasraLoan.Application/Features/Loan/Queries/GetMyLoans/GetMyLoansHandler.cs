using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans
{
    public class GetMyLoansHandler
    : IRequestHandler<GetMyLoansQuery, List<GetMyLoansResponse>>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyLoansHandler(
            ILoanRequestRepository loanRequestRepository,
            ICurrentUserService currentUserService)
        {
            _loanRequestRepository = loanRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<GetMyLoansResponse>> Handle(
            GetMyLoansQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = _currentUserService.UserId;

            var loans = await _loanRequestRepository
                .GetByEmployeeIdAsync(employeeId);

            var count = loans.Count;

            Guid? firstEmployeeId = null;

            if (count > 0)
            {
                firstEmployeeId = loans[0].EmployeeId;
            }

            var result = loans.Select(loan => new GetMyLoansResponse
            {
                Id = loan.Id,
                LoanType = loan.LoanType.Name,
                RequestedAmount = loan.RequestedAmount,
                ApprovedAmount = loan.ApprovedAmount,
                InstallmentCount = loan.InstallmentCount,
                Status = loan.Status.ToString()
            }).ToList();

            return result;
        }
    }
}