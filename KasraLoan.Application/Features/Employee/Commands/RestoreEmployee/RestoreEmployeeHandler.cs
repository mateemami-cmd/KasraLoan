using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.RestoreEmployee
{
    public class RestoreEmployeeHandler
        : IRequestHandler<RestoreEmployeeCommand, RestoreEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAuditLogService _auditLogService;

        public RestoreEmployeeHandler(
            IEmployeeRepository employeeRepository,
            IAuditLogService auditLogService)
        {
            _employeeRepository = employeeRepository;
            _auditLogService = auditLogService;
        }

        public async Task<RestoreEmployeeResponse> Handle(
            RestoreEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            if (!employee.IsDeleted)
                throw new BusinessRuleException("این کارمند حذف نشده است.");

            employee.IsDeleted = false;
            employee.DeletedAt = null;
            // حساب را غیرفعال نگه می‌داریم تا ادمین آگاهانه فعالش کند.

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                employee.Id,
                null,
                "RestoreEmployee",
                "Employee restored (kept inactive)");

            return new RestoreEmployeeResponse
            {
                EmployeeId = employee.Id,
                Message = "کارمند بازگردانده شد (غیرفعال). برای دسترسیِ ورود، حسابش را فعال کنید."
            };
        }
    }
}
