using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanById
{
    public class GetLoanByIdHandler
    : IRequestHandler<GetLoanByIdQuery, GetLoanByIdResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;

        public GetLoanByIdHandler(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<GetLoanByIdResponse> Handle(
            GetLoanByIdQuery request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

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