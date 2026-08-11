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
    : IRequestHandler<GetAllLoansQuery, GetAllLoansResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;

        public GetAllLoansHandler(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<GetAllLoansResponse> Handle(
            GetAllLoansQuery request,
            CancellationToken cancellationToken)
        {
            var loans = await _loanRequestRepository.GetPagedAsync(
                request.Page, request.PageSize, request.Status, request.Search);

            var totalCount = await _loanRequestRepository.GetPagedCountAsync(
                request.Status, request.Search);

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