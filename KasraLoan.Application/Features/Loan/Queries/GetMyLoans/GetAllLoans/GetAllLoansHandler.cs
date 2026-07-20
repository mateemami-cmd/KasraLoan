using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans.GetAllLoans
{
    public class GetAllLoansHandler
    : IRequestHandler<GetAllLoansQuery, List<GetAllLoansResponse>>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;

        public GetAllLoansHandler(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<List<GetAllLoansResponse>> Handle(
            GetAllLoansQuery request,
            CancellationToken cancellationToken)
        {
            var loans = await _loanRequestRepository.GetAllAsync();

            return loans.Select(x => new GetAllLoansResponse
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                LoanTypeId = x.LoanTypeId,
                RequestedAmount = x.RequestedAmount,
                ApprovedAmount = x.ApprovedAmount,
                InstallmentCount = x.InstallmentCount,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt
            }).ToList();
        }
    }
}