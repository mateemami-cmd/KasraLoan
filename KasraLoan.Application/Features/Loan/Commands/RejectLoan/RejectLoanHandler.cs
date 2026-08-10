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

namespace KasraLoan.Application.Features.Loan.Commands.RejectLoan
{
    public class RejectLoanHandler : IRequestHandler<RejectLoanCommand, RejectLoanResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;

        public RejectLoanHandler(ILoanRequestRepository loanRequestRepository, IAuditLogService auditLogService, INotificationService notificationService)
        {
            _loanRequestRepository = loanRequestRepository;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
        }

        public async Task<RejectLoanResponse> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

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