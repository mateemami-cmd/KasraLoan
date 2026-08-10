using FluentAssertions;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Features.Employee.Commands.SetEmploymentStatus;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KasraLoan.Tests.Employment;

public class SetEmploymentStatusHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEmploymentStatusChangeRepository> _statusChangeRepository = new();
    private readonly Mock<ILoanRequestRepository> _loanRequestRepository = new();
    private readonly Mock<IPayrollCalendarService> _payrollCalendar = new();
    private readonly Mock<ILoanSettlementService> _loanSettlementService = new();
    private readonly Mock<IAuditLogService> _auditLogService = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private readonly SetEmploymentStatusHandler _handler;

    private readonly Guid _employeeId = Guid.NewGuid();

    public SetEmploymentStatusHandlerTests()
    {
        _payrollCalendar.Setup(x => x.ToPersianDateString(It.IsAny<DateTime>()))
            .Returns("1405/05/19");
        _payrollCalendar.Setup(x => x.DescribeEmploymentChangeWindow())
            .Returns("از روز 28 هر ماه شمسی تا روز 1 ماه بعد");

        _currentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _handler = new SetEmploymentStatusHandler(
            _employeeRepository.Object,
            _statusChangeRepository.Object,
            _loanRequestRepository.Object,
            _payrollCalendar.Object,
            _loanSettlementService.Object,
            _auditLogService.Object,
            _notificationService.Object,
            _currentUserService.Object);
    }

    private Domain.Entities.Employee GivenEmployee(
        EmploymentStatus status = EmploymentStatus.Active)
    {
        var employee = new Domain.Entities.Employee
        {
            Id = _employeeId,
            FirstName = "T",
            LastName = "T",
            EmploymentStatus = status,
            IsActive = true
        };

        _employeeRepository.Setup(x => x.GetByIdAsync(_employeeId)).ReturnsAsync(employee);

        return employee;
    }

    private SetEmploymentStatusCommand Command(string status, string reason = "استعفا")
        => new()
        {
            EmployeeId = _employeeId,
            Request = new SetEmploymentStatusRequestDto { Status = status, Reason = reason }
        };

    [Fact]
    public async Task Terminate_Is_Rejected_Outside_The_Payroll_Window()
    {
        GivenEmployee();
        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(false);

        var act = () => _handler.Handle(Command("Terminated"), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*لیست حقوق*");
    }

    [Fact]
    public async Task Terminate_Succeeds_Inside_The_Payroll_Window()
    {
        var employee = GivenEmployee();
        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(true);

        var result = await _handler.Handle(Command("Terminated"), CancellationToken.None);

        result.Status.Should().Be("Terminated");
        employee.EmploymentStatus.Should().Be(EmploymentStatus.Terminated);
        employee.TerminationDate.Should().NotBeNull();

        // حساب کاربری نباید بسته شود؛ کارمند باید بتواند اقساطش را ببیند.
        employee.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Reactivation_Is_Allowed_Outside_The_Window()
    {
        var employee = GivenEmployee(EmploymentStatus.Terminated);
        employee.TerminationDate = DateTime.UtcNow.AddDays(-30);

        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(false);

        var result = await _handler.Handle(
            Command("Active", "بازگشت به کار"), CancellationToken.None);

        result.Status.Should().Be("Active");
        employee.EmploymentStatus.Should().Be(EmploymentStatus.Active);
        employee.TerminationDate.Should().BeNull();
    }

    [Fact]
    public async Task Setting_The_Same_Status_Is_Rejected()
    {
        GivenEmployee(EmploymentStatus.Active);
        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(true);

        var act = () => _handler.Handle(Command("Active"), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Termination_Demands_Full_Settlement_Of_Outstanding_Loan()
    {
        GivenEmployee();
        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(true);
        _loanRequestRepository.Setup(x => x.HasActiveLoanAsync(_employeeId)).ReturnsAsync(true);

        _loanSettlementService
            .Setup(x => x.DemandSettlementForEmployeeAsync(_employeeId, It.IsAny<string>()))
            .ReturnsAsync(new LoanSettlementDemandDto
            {
                TotalOutstandingAmount = 80_000_000,
                RemainingInstallments = 8,
                SettlementDueDate = DateTime.UtcNow.AddDays(30),
                SettlementDueDatePersian = "1405/06/18"
            });

        var result = await _handler.Handle(Command("Terminated"), CancellationToken.None);

        result.Settlement.Should().NotBeNull();
        result.Settlement!.TotalOutstandingAmount.Should().Be(80_000_000);
        result.Message.Should().Contain("80,000,000");
        result.Message.Should().Contain("1405/06/18");
    }

    [Fact]
    public async Task Termination_Without_An_Open_Loan_Demands_Nothing()
    {
        GivenEmployee();
        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(true);
        _loanSettlementService
            .Setup(x => x.DemandSettlementForEmployeeAsync(_employeeId, It.IsAny<string>()))
            .ReturnsAsync((LoanSettlementDemandDto?)null);

        var result = await _handler.Handle(Command("Terminated"), CancellationToken.None);

        result.Settlement.Should().BeNull();
        result.Message.Should().NotContain("مانده");
    }

    [Fact]
    public async Task Reactivation_Never_Demands_Settlement()
    {
        GivenEmployee(EmploymentStatus.Terminated);

        await _handler.Handle(Command("Active", "بازگشت به کار"), CancellationToken.None);

        _loanSettlementService.Verify(
            x => x.DemandSettlementForEmployeeAsync(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Change_Is_Recorded_In_History_And_Audit_Log()
    {
        GivenEmployee();
        _payrollCalendar.Setup(x => x.IsWithinEmploymentChangeWindow(It.IsAny<DateTime>()))
            .Returns(true);

        await _handler.Handle(Command("Terminated", "پایان قرارداد"), CancellationToken.None);

        _statusChangeRepository.Verify(x => x.AddAsync(It.Is<EmploymentStatusChange>(c =>
            c.FromStatus == EmploymentStatus.Active &&
            c.ToStatus == EmploymentStatus.Terminated &&
            c.Reason == "پایان قرارداد")), Times.Once);

        _auditLogService.Verify(x => x.LogAsync(
            _employeeId, null, "SetEmploymentStatus", It.IsAny<string>()), Times.Once);

        _notificationService.Verify(x => x.SendAsync(
            _employeeId, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Unknown_Employee_Throws_Not_Found()
    {
        _employeeRepository.Setup(x => x.GetByIdAsync(_employeeId))
            .ReturnsAsync((Domain.Entities.Employee?)null);

        var act = () => _handler.Handle(Command("Terminated"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
