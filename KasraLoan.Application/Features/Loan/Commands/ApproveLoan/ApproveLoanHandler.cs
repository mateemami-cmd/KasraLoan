using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.ApproveLoan
{
    public class ApproveLoanHandler
        : IRequestHandler<ApproveLoanCommand, ApproveLoanResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILoanInstallmentService _loanInstallmentService;

        public ApproveLoanHandler(
            ILoanRequestRepository loanRequestRepository,
            IAuditLogService auditLogService,
            ILoanInstallmentService loanInstallmentService)
        {
            _loanRequestRepository = loanRequestRepository;
            _auditLogService = auditLogService;
            _loanInstallmentService = loanInstallmentService;
        }

        public async Task<ApproveLoanResponse> Handle(
            ApproveLoanCommand request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository
                .GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            if (loan.Status != LoanStatus.Pending)
                throw new Exception("این وام قابل تأیید نیست");

            loan.Status = LoanStatus.Approved;

            loan.TotalPayableAmount = loan.ApprovedAmount;

            loan.MonthlyPaymentAmount =
                loan.TotalPayableAmount / loan.InstallmentCount;

            loan.ApprovedAt = DateTime.UtcNow;

            await _loanRequestRepository.SaveChangesAsync();

            await _loanInstallmentService.CreateInstallmentsAsync(loan.Id);

            await _auditLogService.LogAsync(
                loan.EmployeeId,
                loan.Id,
                "ApproveLoan",
                $"Loan approved. Amount: {loan.ApprovedAmount}");

            return new ApproveLoanResponse
            {
                Message = "وام تأیید شد"
            };
        }
    }
}