using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanOutstanding
{
    public class GetLoanOutstandingHandler
        : IRequestHandler<GetLoanOutstandingQuery, LoanOutstandingDto>
    {
        private readonly ILoanSettlementService _loanSettlementService;
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLoanOutstandingHandler(
            ILoanSettlementService loanSettlementService,
            ILoanRequestRepository loanRequestRepository,
            ICurrentUserService currentUserService)
        {
            _loanSettlementService = loanSettlementService;
            _loanRequestRepository = loanRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<LoanOutstandingDto> Handle(
            GetLoanOutstandingQuery request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد.");

            var isAdmin = string.Equals(
                _currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && loan.EmployeeId != _currentUserService.UserId)
                throw new ForbiddenAccessException("شما اجازه‌ی مشاهده‌ی مانده‌ی این وام را ندارید.");

            return await _loanSettlementService.GetOutstandingAsync(request.LoanRequestId);
        }
    }
}
