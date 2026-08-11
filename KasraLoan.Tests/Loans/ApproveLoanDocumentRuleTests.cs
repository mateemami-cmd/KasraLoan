using FluentAssertions;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Features.Loan.Commands.ApproveLoan;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules.Implementations;
using KasraLoan.Application.Services;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KasraLoan.Tests.Loans;

public class ApproveLoanDocumentRuleTests
{
    private readonly Mock<ILoanRequestRepository> _loans = new();
    private readonly Mock<IAuditLogService> _audit = new();
    private readonly Mock<ILoanInstallmentService> _installments = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<ILoanDocumentRepository> _documents = new();

    private readonly ApproveLoanHandler _sut;

    public ApproveLoanDocumentRuleTests()
    {
        _sut = new ApproveLoanHandler(
            _loans.Object,
            _audit.Object,
            _installments.Object,
            _notifications.Object,
            new LoanCalculationService(),
            _documents.Object);
    }

    private LoanRequest GivenPendingLoan(bool requiresDocument)
    {
        var loan = new LoanRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            Status = LoanStatus.Pending,
            ApprovedAmount = 200_000_000,
            InstallmentCount = 24,
            AnnualFeePercent = 5,
            RequiresDocument = requiresDocument,
            RequiredDocumentDescription = requiresDocument ? "تصویر سند ازدواج" : null,
        };

        _loans.Setup(x => x.GetByIdAsync(loan.Id)).ReturnsAsync(loan);

        return loan;
    }

    [Fact]
    public async Task Approval_Is_Blocked_When_A_Required_Document_Is_Missing()
    {
        var loan = GivenPendingLoan(requiresDocument: true);
        _documents.Setup(x => x.ExistsAsync(loan.Id)).ReturnsAsync(false);

        var act = () => _sut.Handle(
            new ApproveLoanCommand { LoanRequestId = loan.Id }, CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*تصویر سند ازدواج*");

        loan.Status.Should().Be(LoanStatus.Pending);
        _installments.Verify(x => x.CreateInstallmentsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Approval_Succeeds_Once_The_Document_Is_Uploaded()
    {
        var loan = GivenPendingLoan(requiresDocument: true);
        _documents.Setup(x => x.ExistsAsync(loan.Id)).ReturnsAsync(true);

        await _sut.Handle(new ApproveLoanCommand { LoanRequestId = loan.Id }, CancellationToken.None);

        loan.Status.Should().Be(LoanStatus.Approved);
        _installments.Verify(x => x.CreateInstallmentsAsync(loan.Id), Times.Once);
    }

    [Fact]
    public async Task Loans_That_Need_No_Document_Approve_Without_One()
    {
        var loan = GivenPendingLoan(requiresDocument: false);

        await _sut.Handle(new ApproveLoanCommand { LoanRequestId = loan.Id }, CancellationToken.None);

        loan.Status.Should().Be(LoanStatus.Approved);
        _documents.Verify(x => x.ExistsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void Marriage_Loan_Rule_Requires_A_Marriage_Certificate()
    {
        var rule = new MarriageLoanRule();

        var result = rule.Evaluate(new Application.LoanRules.LoanRuleContext
        {
            Employee = new Employee(),
            LoanType = new LoanType { Type = LoanTypeEnum.MarriageLoan },
            RequestedAmount = 100_000_000,
            EmployeeScore = 6000,
        });

        result.RequiresDocument.Should().BeTrue();
        result.RequiredDocumentDescription.Should().Contain("سند ازدواج");
    }

    [Fact]
    public void Travel_Loan_Rule_Requires_No_Document()
    {
        var rule = new TravelLoanRule();

        var result = rule.Evaluate(new Application.LoanRules.LoanRuleContext
        {
            Employee = new Employee(),
            LoanType = new LoanType { Type = LoanTypeEnum.TravelLoan },
            RequestedAmount = 10_000_000,
            EmployeeScore = 6000,
        });

        result.RequiresDocument.Should().BeFalse();
    }
}
