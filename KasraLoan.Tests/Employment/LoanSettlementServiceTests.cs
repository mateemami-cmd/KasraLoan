using FluentAssertions;
using KasraLoan.Application.Common.Payroll;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KasraLoan.Tests.Employment;

public class LoanSettlementServiceTests
{
    private readonly Mock<ILoanRequestRepository> _loanRepository = new();
    private readonly Mock<IPayrollCalendarService> _payrollCalendar = new();

    private readonly LoanSettlementService _sut;

    private readonly Guid _employeeId = Guid.NewGuid();

    public LoanSettlementServiceTests()
    {
        _payrollCalendar.Setup(x => x.ToPersianDateString(It.IsAny<DateTime>()))
            .Returns("1405/06/18");

        _sut = new LoanSettlementService(
            _loanRepository.Object,
            _payrollCalendar.Object,
            Options.Create(new PayrollCycleOptions()));
    }

    /// <summary>وامی با ۱۲ قسط ۱۰ میلیونی که تعدادی از آن‌ها پرداخت شده‌اند.</summary>
    private static LoanRequest LoanWith(int totalInstallments, int paidCount)
    {
        var loan = new LoanRequest
        {
            Id = Guid.NewGuid(),
            Status = LoanStatus.Active,
            TotalPayableAmount = totalInstallments * 10_000_000L
        };

        loan.LoanInstallments = Enumerable.Range(1, totalInstallments)
            .Select(i => new LoanInstallment
            {
                Id = Guid.NewGuid(),
                LoanRequestId = loan.Id,
                InstallmentNumber = i,
                Amount = 10_000_000m,
                IsPaid = i <= paidCount
            })
            .ToList();

        return loan;
    }

    [Fact]
    public async Task Outstanding_Is_The_Sum_Of_Unpaid_Installments()
    {
        var loan = LoanWith(totalInstallments: 12, paidCount: 4);
        _loanRepository.Setup(x => x.GetByIdAsync(loan.Id)).ReturnsAsync(loan);

        var result = await _sut.GetOutstandingAsync(loan.Id);

        result.OutstandingAmount.Should().Be(80_000_000);
        result.PaidAmount.Should().Be(40_000_000);
        result.RemainingInstallments.Should().Be(8);
        result.PaidInstallments.Should().Be(4);
    }

    [Fact]
    public async Task Demand_Marks_Every_Open_Loan_With_Amount_And_Deadline()
    {
        var loan = LoanWith(12, 4);
        _loanRepository.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest> { loan });

        var result = await _sut.DemandSettlementForEmployeeAsync(_employeeId, "پایان همکاری");

        result.Should().NotBeNull();
        result!.TotalOutstandingAmount.Should().Be(80_000_000);
        result.RemainingInstallments.Should().Be(8);

        loan.IsSettlementDemanded.Should().BeTrue();
        loan.SettlementAmount.Should().Be(80_000_000);
        loan.SettlementReason.Should().Be("پایان همکاری");

        // مهلت پیش‌فرض ۳۰ روز است، نه «همین امروز».
        loan.SettlementDueDate.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));

        _loanRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Installments_Are_Kept_So_The_Schedule_History_Survives()
    {
        var loan = LoanWith(12, 4);
        _loanRepository.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest> { loan });

        await _sut.DemandSettlementForEmployeeAsync(_employeeId, "پایان همکاری");

        loan.LoanInstallments.Should().HaveCount(12);
        loan.LoanInstallments.Count(x => x.IsPaid).Should().Be(4);
    }

    [Fact]
    public async Task Fully_Paid_Loan_Is_Not_Demanded()
    {
        var loan = LoanWith(totalInstallments: 12, paidCount: 12);
        _loanRepository.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest> { loan });

        var result = await _sut.DemandSettlementForEmployeeAsync(_employeeId, "پایان همکاری");

        result.Should().BeNull();
        loan.IsSettlementDemanded.Should().BeFalse();
    }

    [Fact]
    public async Task Employee_Without_Open_Loans_Returns_Null()
    {
        _loanRepository.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest>());

        var result = await _sut.DemandSettlementForEmployeeAsync(_employeeId, "پایان همکاری");

        result.Should().BeNull();
        _loanRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Multiple_Open_Loans_Are_Summed()
    {
        var first = LoanWith(12, 4);   // ۸۰ م مانده
        var second = LoanWith(6, 1);   // ۵۰ م مانده

        _loanRepository.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest> { first, second });

        var result = await _sut.DemandSettlementForEmployeeAsync(_employeeId, "پایان همکاری");

        result!.TotalOutstandingAmount.Should().Be(130_000_000);
        result.LoanRequestIds.Should().HaveCount(2);
    }
}
