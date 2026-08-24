using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.DeleteEmployee
{
    public class DeleteEmployeeHandler
        : IRequestHandler<DeleteEmployeeCommand, DeleteEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public DeleteEmployeeHandler(
            IEmployeeRepository employeeRepository,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task<DeleteEmployeeResponse> Handle(
            DeleteEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            if (employee.Id == _currentUserService.UserId)
                throw new BusinessRuleException("نمی‌توانید حساب خودتان را حذف کنید.");

            if (employee.IsDeleted)
                throw new BusinessRuleException("این کارمند از قبل حذف شده است.");

            // حذفِ نرم: ردیف و سوابق دست‌نخورده می‌مانند؛ فقط علامتِ حذف می‌خورد و
            // دسترسیِ ورود/وام قطع می‌شود.
            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            employee.IsActive = false;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                employee.Id,
                null,
                "DeleteEmployee",
                "Employee soft-deleted; records preserved");

            return new DeleteEmployeeResponse
            {
                EmployeeId = employee.Id,
                Message = "کارمند حذف شد. سوابق و وام‌های او در سیستم حفظ شده‌اند."
            };
        }
    }
}
