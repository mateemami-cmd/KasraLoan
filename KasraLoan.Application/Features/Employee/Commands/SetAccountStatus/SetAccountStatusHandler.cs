using System;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetAccountStatus
{
    public class SetAccountStatusHandler
        : IRequestHandler<SetAccountStatusCommand, SetAccountStatusResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public SetAccountStatusHandler(
            IEmployeeRepository employeeRepository,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task<SetAccountStatusResponse> Handle(
            SetAccountStatusCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            // جلوگیری از این‌که ادمین حساب خودش را قفل کند و از سامانه بیرون بماند.
            if (employee.Id == _currentUserService.UserId)
                throw new BusinessRuleException(
                    "نمی‌توانید وضعیت حساب کاربری خودتان را تغییر دهید.");

            var isActive = request.Request.IsActive;

            if (employee.IsActive == isActive)
                throw new BusinessRuleException(
                    isActive ? "این حساب از قبل فعال است." : "این حساب از قبل غیرفعال است.");

            employee.IsActive = isActive;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                employee.Id,
                null,
                "SetAccountStatus",
                isActive ? "Account activated" : "Account deactivated");

            return new SetAccountStatusResponse
            {
                EmployeeId = employee.Id,
                IsActive = isActive,
                Message = isActive
                    ? "حساب کاربری فعال شد."
                    : "حساب کاربری غیرفعال شد؛ کاربر دیگر نمی‌تواند وارد شود یا وام بگیرد."
            };
        }
    }
}
