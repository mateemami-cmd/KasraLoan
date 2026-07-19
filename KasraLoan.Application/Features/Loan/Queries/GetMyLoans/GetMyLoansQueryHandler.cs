//using KasraLoan.Application.Interfaces.Repositories;
//using KasraLoan.Application.Services.Auth;
//using MediatR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans
//{
//    public class GetMyLoansHandler
//    : IRequestHandler<GetMyLoansQuery, List<GetMyLoansResponse>>
//    {
//        private readonly ILoanRequestRepository _loanRequestRepository;
//        private readonly ICurrentUserService _currentUserService;

//        public GetMyLoansHandler(
//            ILoanRequestRepository loanRequestRepository,
//            ICurrentUserService currentUserService)
//        {
//            _loanRequestRepository = loanRequestRepository;
//            _currentUserService = currentUserService;
//        }

//        public async Task<List<GetMyLoansResponse>> Handle(
//            GetMyLoansQuery request,
//            CancellationToken cancellationToken)
//        {
//            var employeeId = _currentUserService.UserId;

//            var loans = await _loanRequestRepository
//                .GetByEmployeeIdAsync(employeeId);

//            return loans.Select(x => new GetMyLoansResponse
//            {
//                Id = x.Id,
//                LoanType = x.LoanType.Name,
//                RequestedAmount = x.RequestedAmount,
//                ApprovedAmount = x.ApprovedAmount,
//                InstallmentCount = x.InstallmentCount,
//                Status = x.Status.ToString()
//            }).ToList();
//        }
//    }
//}