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

namespace KasraLoan.Application.Features.LoanPermission.Commands.CreateLoanPermissionRequest
{
    public class CreateLoanPermissionRequestHandler
        : IRequestHandler<CreateLoanPermissionRequestCommand, CreateLoanPermissionRequestResponse>
    {
        private readonly ILoanPermissionRequestRepository _permissionRequestRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public CreateLoanPermissionRequestHandler(
            ILoanPermissionRequestRepository permissionRequestRepository,
            ILoanTypeRepository loanTypeRepository,
            IEmployeeRepository employeeRepository,
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _permissionRequestRepository = permissionRequestRepository;
            _loanTypeRepository = loanTypeRepository;
            _employeeRepository = employeeRepository;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<CreateLoanPermissionRequestResponse> Handle(
            CreateLoanPermissionRequestCommand request,
            CancellationToken cancellationToken)
        {
            var employeeId = _currentUserService.UserId;

            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var loanType = await _loanTypeRepository.GetByIdAsync(request.Request.LoanTypeId);

            if (loanType == null)
                throw new KeyNotFoundException("Loan type not found");

            if (!loanType.IsActive)
                throw new BusinessRuleException("این نوع وام در حال حاضر غیرفعال است و امکان درخواست مجوز برای آن وجود ندارد.");

            // اگر کارمند همین حالا یک درخواست مجوز در انتظار بررسی دارد،
            // نباید بتواند درخواست تکراری ثبت کند.
            var hasPending = await _permissionRequestRepository.HasPendingRequestAsync(employeeId);

            if (hasPending)
                throw new BusinessRuleException("شما یک درخواست مجوز در انتظار بررسی دارید و تا تعیین تکلیف آن نمی‌توانید درخواست جدیدی ثبت کنید.");

            var permissionRequest = new Domain.Entities.LoanPermissionRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                LoanTypeId = loanType.Id,
                Reason = request.Request.Reason,
                Status = LoanPermissionRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _permissionRequestRepository.AddAsync(permissionRequest);
            await _permissionRequestRepository.SaveChangesAsync();

            await _notificationService.SendAsync(
                employeeId,
                "ثبت درخواست مجوز وام",
                "درخواست مجوز وام شما با موفقیت ثبت شد و در انتظار بررسی ادمین است.");

            return new CreateLoanPermissionRequestResponse
            {
                RequestId = permissionRequest.Id,
                Message = "درخواست مجوز وام با موفقیت ثبت شد."
            };
        }
    }
}
