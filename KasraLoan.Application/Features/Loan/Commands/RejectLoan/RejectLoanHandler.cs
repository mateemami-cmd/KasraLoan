using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.RejectLoan
{
    public class RejectLoanHandler : IRequestHandler<RejectLoanCommand, RejectLoanResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public RejectLoanHandler(ILoanRequestRepository loanRequestRepository, IAuditLogService auditLogService, INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _loanRequestRepository = loanRequestRepository;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        public async Task<RejectLoanResponse> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            // ادمین وام فقط می‌تواند درخواست‌های وامِ نوعِ خودش را رد کند.
            if (!_currentUserService.CanManageLoanType(loan.LoanTypeId))
                throw new BusinessRuleException("شما به این نوع وام دسترسی ندارید.");

            if (loan.Status != LoanStatus.Pending)
                throw new BusinessRuleException("این وام قابل رد نیست");

            loan.Status = LoanStatus.Rejected;

            var reason = string.IsNullOrWhiteSpace(request.RejectReason)
                ? null
                : request.RejectReason.Trim();

            loan.RejectReason = reason;

            await _loanRequestRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                loan.EmployeeId,
                loan.Id,
                "RejectLoan",
                $"Loan rejected by admin.{(reason != null ? $" Reason: {reason}" : string.Empty)}");

            await _notificationService.SendAsync(
                loan.EmployeeId,
                "رد درخواست وام",
                reason != null
                    ? $"درخواست وام شما رد شد. دلیل: {reason}"
                    : "درخواست وام شما توسط کارشناس رد شد.");

            return new RejectLoanResponse
            {
                Message = "وام رد شد"
            };
        }
    }
}