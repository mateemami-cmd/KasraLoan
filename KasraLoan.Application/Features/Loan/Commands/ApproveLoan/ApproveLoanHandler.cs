using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.ApproveLoan
{
    public class ApproveLoanHandler : IRequestHandler<ApproveLoanCommand, ApproveLoanResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly INotificationService _notificationService;

        public ApproveLoanHandler(ILoanRequestRepository loanRequestRepository, IAuditLogService auditLogService, ILoanInstallmentService loanInstallmentService, INotificationService notificationService)
        {
            _loanRequestRepository = loanRequestRepository;
            _auditLogService = auditLogService;
            _loanInstallmentService = loanInstallmentService;
            _notificationService = notificationService;
        }

        public async Task<ApproveLoanResponse> Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            if (loan.Status != LoanStatus.Pending)
                throw new Exception("این وام قابل تأیید نیست");

            loan.Status = LoanStatus.Approved;

            var totalFee = loan.ApprovedAmount * (loan.MonthlyFeePercent / 100m) * loan.InstallmentCount;

            loan.TotalPayableAmount = loan.ApprovedAmount + (int)Math.Round(totalFee);

            loan.MonthlyPaymentAmount = Math.Round((decimal)loan.TotalPayableAmount / loan.InstallmentCount, 0);

            loan.ApprovedAt = DateTime.UtcNow;

            await _loanRequestRepository.SaveChangesAsync();

            await _loanInstallmentService.CreateInstallmentsAsync(loan.Id);

            await _auditLogService.LogAsync(
                loan.EmployeeId,
                loan.Id,
                "ApproveLoan",
                $"Loan approved. Amount: {loan.ApprovedAmount}");

            await _notificationService.SendAsync(
                loan.EmployeeId,
                "تأیید وام",
                $"وام شما به مبلغ {loan.ApprovedAmount:N0} تومان تأیید شد.");

            return new ApproveLoanResponse
            {
                Message = "وام تأیید شد"
            };
        }
    }
}