using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Common.Exceptions;
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

    //IRequestHandler<TRequest, TResponse>
    public class ApproveLoanHandler : IRequestHandler<ApproveLoanCommand, ApproveLoanResponse>
    {

        //Constructor Injection
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly INotificationService _notificationService;
        private readonly ILoanCalculationService _loanCalculationService;
        private readonly ILoanDocumentRepository _loanDocumentRepository;

        public ApproveLoanHandler(
            ILoanRequestRepository loanRequestRepository,
            IAuditLogService auditLogService,
            ILoanInstallmentService loanInstallmentService,
            INotificationService notificationService,
            ILoanCalculationService loanCalculationService,
            ILoanDocumentRepository loanDocumentRepository)
        {
            _loanRequestRepository = loanRequestRepository;
            _auditLogService = auditLogService;
            _loanInstallmentService = loanInstallmentService;
            _notificationService = notificationService;
            _loanCalculationService = loanCalculationService;
            _loanDocumentRepository = loanDocumentRepository;
        }

        public async Task<ApproveLoanResponse> Handle(
            ApproveLoanCommand request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            if (loan.Status != LoanStatus.Pending)
                throw new BusinessRuleException("این وام قابل تأیید نیست");

            // وامی که مدرک لازم دارد نباید بدون مدرک تأیید شود. تا پیش از این،
            // ادمین می‌توانست وام ازدواج ۲۰۰ میلیونی را بدون هیچ سندی تأیید کند.
            if (loan.RequiresDocument && !await _loanDocumentRepository.ExistsAsync(loan.Id))
            {
                throw new BusinessRuleException(
                    $"برای تأیید این وام، ابتدا باید {loan.RequiredDocumentDescription ?? "مدرک لازم"} " +
                    "توسط کارمند بارگذاری شود.");
            }

            loan.Status = LoanStatus.Approved;

            loan.TotalPayableAmount = _loanCalculationService.CalculateTotalPayable(
                loan.ApprovedAmount,
                loan.AnnualFeePercent,
                loan.InstallmentCount);

            loan.MonthlyPaymentAmount = _loanCalculationService.CalculateMonthlyPayment(
                loan.TotalPayableAmount,
                loan.InstallmentCount);

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