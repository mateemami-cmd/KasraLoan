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

            Console.WriteLine($"Current User Id: {employeeId}");

            var loans = await _loanRequestRepository
                .GetByEmployeeIdAsync(employeeId);

            Console.WriteLine($"Loans Count: {loans.Count}");

            var result = new List<GetMyLoansResponse>();

            //foreach (var loan in loans)
            //{
            //    var loanType = await _loanTypeRepository
            //        .GetByIdAsync(loan.LoanTypeId);

            //    result.Add(new GetMyLoansResponse
            //    {
            //        Id = loan.Id,
            //        LoanType = loanType?.Name ?? "",
            //        RequestedAmount = loan.RequestedAmount,
            //        ApprovedAmount = loan.ApprovedAmount,
            //        InstallmentCount = loan.InstallmentCount,
            //        Status = loan.Status.ToString()
            //    });
            //}

            return result;
        }
    }
}