using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.SetEmploymentStatus
{
    public class SetEmploymentStatusHandler
        : IRequestHandler<SetEmploymentStatusCommand, EmploymentStatusResponseDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmploymentStatusChangeRepository _statusChangeRepository;
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IPayrollCalendarService _payrollCalendar;
        private readonly ILoanSettlementService _loanSettlementService;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public SetEmploymentStatusHandler(
            IEmployeeRepository employeeRepository,
            IEmploymentStatusChangeRepository statusChangeRepository,
            ILoanRequestRepository loanRequestRepository,
            IPayrollCalendarService payrollCalendar,
            ILoanSettlementService loanSettlementService,
            IAuditLogService auditLogService,
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _statusChangeRepository = statusChangeRepository;
            _loanRequestRepository = loanRequestRepository;
            _payrollCalendar = payrollCalendar;
            _loanSettlementService = loanSettlementService;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        public async Task<EmploymentStatusResponseDto> Handle(
            SetEmploymentStatusCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            var newStatus = Enum.Parse<EmploymentStatus>(request.Request.Status, ignoreCase: true);

            var currentStatus = employee.EmploymentStatus;

            if (newStatus == currentStatus)
            {
                throw new BusinessRuleException(
                    newStatus == EmploymentStatus.Active
                        ? "این کارمند از قبل مشغول به کار است."
                        : "این کارمند از قبل غیرفعال است.");
            }

            var now = DateTime.UtcNow;

            // پنجره فقط روی «غیرفعال کردن» اعمال می‌شود. برگرداندن کارمند به کار
            // هر روزی مجاز است: استخدام و بازگشت به کار تابع چرخه‌ی حقوق نیستند و
            // اگر کسی اشتباهی Terminated زده باشد، نباید تا آخر ماه گیر کند.
            if (newStatus == EmploymentStatus.Terminated
                && !_payrollCalendar.IsWithinEmploymentChangeWindow(now))
            {
                throw new BusinessRuleException(
                    $"غیرفعال کردن کارمند فقط {_payrollCalendar.DescribeEmploymentChangeWindow()} " +
                    $"امکان‌پذیر است، چون بعد از آن لیست حقوق قطعی شده است. " +
                    $"امروز {_payrollCalendar.ToPersianDateString(now)} است.");
            }

            var hasOutstandingLoan =
                await _loanRequestRepository.HasActiveLoanAsync(employee.Id);

            employee.EmploymentStatus = newStatus;

            employee.TerminationDate =
                newStatus == EmploymentStatus.Terminated ? now : null;

            // با پایان همکاری، حقوقی نمی‌ماند که قسط از آن کسر شود؛ پس کل مانده
            // یکجا مطالبه می‌شود (با مهلت، چون در عمل از تسویه‌حساب پایان کار
            // برداشت می‌شود). اقساط پاک نمی‌شوند تا سابقه‌شان بماند.
            LoanSettlementDemandDto? settlement = null;

            if (newStatus == EmploymentStatus.Terminated)
            {
                settlement = await _loanSettlementService.DemandSettlementForEmployeeAsync(
                    employee.Id,
                    $"پایان همکاری — {request.Request.Reason.Trim()}");
            }

            await _statusChangeRepository.AddAsync(new EmploymentStatusChange
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                FromStatus = currentStatus,
                ToStatus = newStatus,
                Reason = request.Request.Reason.Trim(),
                ChangedByAdminId = _currentUserService.UserId,
                ChangedAt = now
            });

            await _employeeRepository.UpdateAsync(employee);
            await _statusChangeRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                employee.Id,
                null,
                "SetEmploymentStatus",
                $"{currentStatus} -> {newStatus}. Reason: {request.Request.Reason.Trim()}" +
                (settlement != null
                    ? $" | Settlement demanded: {settlement.TotalOutstandingAmount} due {settlement.SettlementDueDate:yyyy-MM-dd}"
                    : string.Empty));

            var message = BuildMessage(newStatus, settlement);

            await _notificationService.SendAsync(
                employee.Id,
                newStatus == EmploymentStatus.Terminated ? "پایان همکاری" : "بازگشت به کار",
                message);

            return new EmploymentStatusResponseDto
            {
                EmployeeId = employee.Id,
                Status = newStatus.ToString(),
                TerminationDate = employee.TerminationDate,
                ChangedAtPersian = _payrollCalendar.ToPersianDateString(now),
                HasOutstandingLoan = hasOutstandingLoan,
                Settlement = settlement,
                Message = message
            };
        }

        private static string BuildMessage(
            EmploymentStatus newStatus,
            LoanSettlementDemandDto? settlement)
        {
            if (newStatus == EmploymentStatus.Active)
                return "وضعیت اشتغال شما به «مشغول به کار» تغییر کرد.";

            if (settlement == null)
                return "وضعیت اشتغال شما به «پایان همکاری» تغییر کرد.";

            // کارمندِ رفته حسابش بسته نمی‌شود؛ باید بتواند مانده‌اش را ببیند و بپردازد.
            return "وضعیت اشتغال شما به «پایان همکاری» تغییر کرد. " +
                   $"مانده‌ی وام شما {settlement.TotalOutstandingAmount:N0} تومان است و " +
                   $"به‌صورت یکجا تا تاریخ {settlement.SettlementDueDatePersian} قابل پرداخت است. " +
                   "دسترسی شما به سامانه برای مشاهده و تسویه باز می‌ماند.";
        }
    }
}
