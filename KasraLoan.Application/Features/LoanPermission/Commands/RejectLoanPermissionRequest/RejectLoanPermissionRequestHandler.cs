using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.RejectLoanPermissionRequest
{
    public class RejectLoanPermissionRequestHandler
        : IRequestHandler<RejectLoanPermissionRequestCommand, RejectLoanPermissionRequestResponse>
    {
        private readonly ILoanPermissionRequestRepository _permissionRequestRepository;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public RejectLoanPermissionRequestHandler(
            ILoanPermissionRequestRepository permissionRequestRepository,
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _permissionRequestRepository = permissionRequestRepository;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        public async Task<RejectLoanPermissionRequestResponse> Handle(
            RejectLoanPermissionRequestCommand request,
            CancellationToken cancellationToken)
        {
            var permissionRequest = await _permissionRequestRepository.GetByIdAsync(request.PermissionRequestId);

            if (permissionRequest == null)
                throw new KeyNotFoundException("Loan permission request not found");

            // ادمین وام فقط می‌تواند درخواست‌های مجوزِ نوع وام خودش را رد کند.
            if (!_currentUserService.CanManageLoanType(permissionRequest.LoanTypeId))
                throw new BusinessRuleException("شما به این نوع وام دسترسی ندارید.");

            if (permissionRequest.Status != LoanPermissionRequestStatus.Pending)
                throw new BusinessRuleException("این درخواست قبلاً بررسی شده است.");

            permissionRequest.Status = LoanPermissionRequestStatus.Rejected;
            permissionRequest.ReviewedAt = DateTime.UtcNow;
            permissionRequest.AdminResponse = request.AdminResponse;

            await _permissionRequestRepository.SaveChangesAsync();

            var loanTypeName = permissionRequest.LoanType?.Name ?? "موردنظر";

            var message = string.IsNullOrWhiteSpace(request.AdminResponse)
                ? $"درخواست مجوز شما برای وام «{loanTypeName}» رد شد."
                : $"درخواست مجوز شما برای وام «{loanTypeName}» رد شد. دلیل: {request.AdminResponse}";

            await _notificationService.SendAsync(
                permissionRequest.EmployeeId,
                "رد درخواست مجوز وام",
                message);

            return new RejectLoanPermissionRequestResponse
            {
                Message = "درخواست مجوز وام رد شد."
            };
        }
    }
}
