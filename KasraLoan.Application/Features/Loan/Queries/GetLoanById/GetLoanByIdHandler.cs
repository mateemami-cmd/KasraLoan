using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanById
{
    public class GetLoanByIdHandler : IRequestHandler<GetLoanByIdQuery, GetLoanByIdResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLoanByIdHandler(ILoanRequestRepository loanRequestRepository, ICurrentUserService currentUserService)
        {
            _loanRequestRepository = loanRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetLoanByIdResponse> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            var isAdmin = string.Equals(_currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && loan.EmployeeId != _currentUserService.UserId)
                throw new ForbiddenAccessException("شما اجازه‌ی مشاهده‌ی این وام را ندارید.");

            return new GetLoanByIdResponse
            {
                Id = loan.Id,
                EmployeeId = loan.EmployeeId,
                EmployeeName = $"{loan.Employee.FirstName} {loan.Employee.LastName}",
                LoanType = loan.LoanType.Name,
                RequestedAmount = loan.RequestedAmount,
                ApprovedAmount = loan.ApprovedAmount,
                InstallmentCount = loan.InstallmentCount,
                Status = loan.Status.ToString(),
                CreatedAt = loan.CreatedAt,
                ApprovedAt = loan.ApprovedAt,
                MonthlyPaymentAmount = loan.MonthlyPaymentAmount,
                TotalPayableAmount = loan.TotalPayableAmount
            };
        }
    }
}