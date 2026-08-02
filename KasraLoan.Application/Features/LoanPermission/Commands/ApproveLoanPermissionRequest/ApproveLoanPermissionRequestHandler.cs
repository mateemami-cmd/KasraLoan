using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.ApproveLoanPermissionRequest
{
    public class ApproveLoanPermissionRequestHandler
        : IRequestHandler<ApproveLoanPermissionRequestCommand, ApproveLoanPermissionRequestResponse>
    {
        private readonly ILoanPermissionRequestRepository _permissionRequestRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly INotificationService _notificationService;

        public ApproveLoanPermissionRequestHandler(
            ILoanPermissionRequestRepository permissionRequestRepository,
            IEmployeeScoreRepository employeeScoreRepository,
            INotificationService notificationService)
        {
            _permissionRequestRepository = permissionRequestRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _notificationService = notificationService;
        }

        public async Task<ApproveLoanPermissionRequestResponse> Handle(
            ApproveLoanPermissionRequestCommand request,
            CancellationToken cancellationToken)
        {
            var permissionRequest = await _permissionRequestRepository.GetByIdAsync(request.PermissionRequestId);

            if (permissionRequest == null)
                throw new KeyNotFoundException("Loan permission request not found");

            if (permissionRequest.Status != LoanPermissionRequestStatus.Pending)
                throw new BusinessRuleException("این درخواست قبلاً بررسی شده است.");

            // فعال‌کردن مجوز یک‌بارمصرف روی حساب کارمند (بدون تغییر امتیاز واقعی‌اش).
            var scoreRecord = await _employeeScoreRepository.GetByEmployeeIdAsync(permissionRequest.EmployeeId);

            if (scoreRecord == null)
            {
                scoreRecord = new EmployeeScore
                {
                    EmployeeId = permissionRequest.EmployeeId,
                    CreatedAt = DateTime.UtcNow
                };

                await _employeeScoreRepository.AddAsync(scoreRecord);
            }

            scoreRecord.HasLoanPermissionOverride = true;
            scoreRecord.PermissionGrantedAt = DateTime.UtcNow;

            permissionRequest.Status = LoanPermissionRequestStatus.Approved;
            permissionRequest.ReviewedAt = DateTime.UtcNow;

            // هر دو موجودیت روی یک DbContext ردیابی می‌شوند؛ یک بار ذخیره کافی است.
            await _permissionRequestRepository.SaveChangesAsync();

            var loanTypeName = permissionRequest.LoanType?.Name ?? "موردنظر";

            await _notificationService.SendAsync(
                permissionRequest.EmployeeId,
                "تأیید درخواست مجوز وام",
                $"درخواست مجوز شما برای وام «{loanTypeName}» تأیید شد. اکنون می‌توانید این وام را درخواست کنید.");

            return new ApproveLoanPermissionRequestResponse
            {
                Message = "درخواست مجوز وام تأیید شد و مجوز یک‌بارمصرف برای کارمند فعال گردید."
            };
        }
    }
}
