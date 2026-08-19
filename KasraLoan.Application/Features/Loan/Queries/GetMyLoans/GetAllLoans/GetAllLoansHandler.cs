using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans.GetAllLoans
{
    public class GetAllLoansHandler
    : IRequestHandler<GetAllLoansQuery, GetAllLoansResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetAllLoansHandler(
            ILoanRequestRepository loanRequestRepository,
            ICurrentUserService currentUserService)
        {
            _loanRequestRepository = loanRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetAllLoansResponse> Handle(
            GetAllLoansQuery request,
            CancellationToken cancellationToken)
        {
            // ادمین ارشد همه را می‌بیند؛ ادمین وام فقط نوع وام خودش.
            var scopeLoanTypeId = _currentUserService.IsSeniorAdmin
                ? (int?)null
                : _currentUserService.ManagedLoanTypeId;

            var loans = await _loanRequestRepository.GetPagedAsync(
                request.Page, request.PageSize, request.Status, request.Search, scopeLoanTypeId);

            var totalCount = await _loanRequestRepository.GetPagedCountAsync(
                request.Status, request.Search, scopeLoanTypeId);

            var items = loans.Select(x => new LoanListItemDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null
                    ? $"{x.Employee.FirstName} {x.Employee.LastName}"
                    : string.Empty,
                EmployeeUsername = x.Employee?.Username ?? string.Empty,
                LoanTypeId = x.LoanTypeId,
                LoanTypeName = x.LoanType?.Name ?? string.Empty,
                RequestedAmount = x.RequestedAmount,
                ApprovedAmount = x.ApprovedAmount,
                InstallmentCount = x.InstallmentCount,
                Status = x.Status.ToString(),
                TotalPayableAmount = x.TotalPayableAmount,
                MonthlyPaymentAmount = x.MonthlyPaymentAmount,
                AnnualFeePercent = x.AnnualFeePercent,
                RequiresDocument = x.RequiresDocument,
                RequiredDocumentDescription = x.RequiredDocumentDescription,
                HasDocument = x.LoanDocuments != null && x.LoanDocuments.Count > 0,
                TotalInstallments = x.LoanInstallments != null ? x.LoanInstallments.Count : 0,
                PaidInstallments = x.LoanInstallments != null
                    ? x.LoanInstallments.Count(i => i.IsPaid)
                    : 0,
                CreatedAt = x.CreatedAt
            })
                .ToList();

            var totalPages = request.PageSize > 0
                ? (int)Math.Ceiling(totalCount / (double)request.PageSize)
                : 0;

            return new GetAllLoansResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }
}