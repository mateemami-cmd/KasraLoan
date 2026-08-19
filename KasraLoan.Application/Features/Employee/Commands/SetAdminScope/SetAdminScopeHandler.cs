using System;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Enums;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetAdminScope
{
    public class SetAdminScopeHandler
        : IRequestHandler<SetAdminScopeCommand, SetAdminScopeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public SetAdminScopeHandler(
            IEmployeeRepository employeeRepository,
            ILoanTypeRepository loanTypeRepository,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _loanTypeRepository = loanTypeRepository;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task<SetAdminScopeResponse> Handle(
            SetAdminScopeCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            if (employee.Role != UserRole.Admin)
                throw new BusinessRuleException("فقط سطح دسترسی ادمین‌ها قابل تغییر است.");

            // جلوگیری از این‌که ادمین ارشد سطح خودش را عوض کند و دسترسی ارشد را از دست بدهد.
            if (employee.Id == _currentUserService.UserId)
                throw new BusinessRuleException("نمی‌توانید سطح دسترسی خودتان را تغییر دهید.");

            string? managedLoanTypeName = null;

            if (request.Request.IsSeniorAdmin)
            {
                employee.IsSeniorAdmin = true;
                employee.ManagedLoanTypeId = null;
            }
            else
            {
                if (!request.Request.ManagedLoanTypeId.HasValue)
                    throw new BusinessRuleException("برای «ادمین وام» باید نوع وام مشخص شود.");

                var loanType = await _loanTypeRepository.GetByIdAsync(request.Request.ManagedLoanTypeId.Value);
                if (loanType == null)
                    throw new BusinessRuleException("نوع وامِ انتخاب‌شده یافت نشد.");

                employee.IsSeniorAdmin = false;
                employee.ManagedLoanTypeId = loanType.Id;
                managedLoanTypeName = loanType.Name;
            }

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                employee.Id,
                null,
                "SetAdminScope",
                employee.IsSeniorAdmin
                    ? "Promoted to senior admin"
                    : $"Assigned as loan admin for loan type {employee.ManagedLoanTypeId}");

            return new SetAdminScopeResponse
            {
                EmployeeId = employee.Id,
                IsSeniorAdmin = employee.IsSeniorAdmin,
                ManagedLoanTypeId = employee.ManagedLoanTypeId,
                ManagedLoanTypeName = managedLoanTypeName,
                Message = employee.IsSeniorAdmin
                    ? "این ادمین به «ادمین ارشد» تبدیل شد."
                    : $"این ادمین مسئول وام «{managedLoanTypeName}» شد. برای اعمال کامل، باید دوباره وارد شود."
            };
        }
    }
}
