using FluentAssertions;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KasraLoan.Tests.Payments;

public class InstallmentPaymentServiceTests
{
    private readonly Mock<IInstallmentPaymentRepository> _payments = new();
    private readonly Mock<ILoanInstallmentRepository> _installments = new();
    private readonly Mock<ILoanRequestRepository> _loans = new();
    private readonly Mock<IPayrollCalendarService> _calendar = new();
    private readonly Mock<IFileStorageService> _files = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<IAuditLogService> _audit = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private readonly InstallmentPaymentService _sut;

    private readonly Guid _employeeId = Guid.NewGuid();

    /// <summary>پرداخت‌هایی که سرویس ساخته، تا بشود وضعیتشان را بررسی کرد.</summary>
    private readonly List<InstallmentPayment> _added = new();

    public InstallmentPaymentServiceTests()
    {
        _calendar.Setup(x => x.ToPersianDateString(It.IsAny<DateTime>())).Returns("1405/05/28");
        _calendar.Setup(x => x.IsWithinPaymentMethodSelectionWindow(It.IsAny<DateTime>())).Returns(true);

        _files.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("/uploads/cheque.jpg");

        _payments.Setup(x => x.AddAsync(It.IsAny<InstallmentPayment>()))
            .Callback<InstallmentPayment>(p => _added.Add(p))
            .Returns(Task.CompletedTask);

        // ادمین ارشد فرض می‌شود؛ به همه‌ی انواع وام دسترسی دارد.
        _currentUser.Setup(x => x.IsSeniorAdmin).Returns(true);
        _currentUser.Setup(x => x.CanManageLoanType(It.IsAny<int>())).Returns(true);

        _sut = new InstallmentPaymentService(
            _payments.Object,
            _installments.Object,
            _loans.Object,
            _calendar.Object,
            new MockPaymentGateway(),
            _files.Object,
            _notifications.Object,
            _audit.Object,
            _currentUser.Object);
    }

    private LoanInstallment GivenInstallment(
        bool isPaid = false,
        int totalInstallments = 12,
        int paidCount = 0,
        DateTime? dueDate = null)
    {
        var loan = new LoanRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = _employeeId,
            Status = LoanStatus.Active,
        };

        loan.LoanInstallments = Enumerable.Range(1, totalInstallments)
            .Select(i => new LoanInstallment
            {
                Id = Guid.NewGuid(),
                LoanRequestId = loan.Id,
                LoanRequest = loan,
                InstallmentNumber = i,
                Amount = 10_000_000m,
                IsPaid = i <= paidCount,
                DueDate = DateTime.UtcNow.AddMonths(i),
            })
            .ToList();

        var target = loan.LoanInstallments.First(i => !i.IsPaid);
        target.IsPaid = isPaid;
        if (dueDate.HasValue) target.DueDate = dueDate.Value;

        _installments.Setup(x => x.GetByIdWithLoanAsync(target.Id)).ReturnsAsync(target);
        _loans.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest> { loan });

        return target;
    }

    // ───────── انتخاب روش ─────────

    [Fact]
    public async Task Selecting_A_Method_Records_It()
    {
        var inst = GivenInstallment();

        var dto = await _sut.SelectMethodAsync(inst.Id, _employeeId, PaymentMethod.PayrollDeduction);

        dto.Method.Should().Be("PayrollDeduction");
        dto.Status.Should().Be("Selected");
        _added.Should().ContainSingle();
    }

    [Fact]
    public async Task Selection_Outside_The_Window_Is_Rejected()
    {
        _calendar.Setup(x => x.IsWithinPaymentMethodSelectionWindow(It.IsAny<DateTime>())).Returns(false);

        var inst = GivenInstallment(dueDate: DateTime.UtcNow.AddDays(20));

        var act = () => _sut.SelectMethodAsync(inst.Id, _employeeId, PaymentMethod.Cheque);

        (await act.Should().ThrowAsync<BusinessRuleException>())
            .WithMessage("*انتخاب روش پرداخت فقط*");
    }

    [Fact]
    public async Task Overdue_Installment_Can_Be_Paid_Outside_The_Window()
    {
        // بستن راه پرداخت روی بدهکارِ معوق به نفع هیچ‌کس نیست.
        _calendar.Setup(x => x.IsWithinPaymentMethodSelectionWindow(It.IsAny<DateTime>())).Returns(false);

        var inst = GivenInstallment(dueDate: DateTime.UtcNow.AddDays(-5));

        var dto = await _sut.SelectMethodAsync(inst.Id, _employeeId, PaymentMethod.OnlineGateway);

        dto.Status.Should().Be("Selected");
    }

    [Fact]
    public async Task Another_Employees_Installment_Is_Forbidden()
    {
        var inst = GivenInstallment();

        var act = () => _sut.SelectMethodAsync(inst.Id, Guid.NewGuid(), PaymentMethod.Cheque);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Cannot_Change_Method_While_A_Cheque_Awaits_Approval()
    {
        var inst = GivenInstallment();

        _payments.Setup(x => x.GetActiveForInstallmentAsync(inst.Id))
            .ReturnsAsync(new InstallmentPayment
            {
                Method = PaymentMethod.Cheque,
                Status = InstallmentPaymentStatus.AwaitingAdminApproval,
            });

        var act = () => _sut.SelectMethodAsync(inst.Id, _employeeId, PaymentMethod.PayrollDeduction);

        (await act.Should().ThrowAsync<BusinessRuleException>()).WithMessage("*در انتظار بررسی*");
    }

    // ───────── چک ─────────

    [Fact]
    public async Task Submitting_A_Cheque_Puts_It_In_The_Admin_Queue()
    {
        var inst = GivenInstallment();

        var dto = await _sut.SubmitChequeAsync(
            inst.Id, _employeeId,
            new SubmitChequeRequestDto { ChequeNumber = "12345", ChequeBankName = "ملت", ChequeDate = DateTime.UtcNow },
            new byte[] { 1, 2, 3 }, "cheque.jpg", "image/jpeg");

        dto.Status.Should().Be("AwaitingAdminApproval");
        dto.ChequeNumber.Should().Be("12345");
        dto.ChequeImageUrl.Should().Be("/uploads/cheque.jpg");

        // چک هنوز پول نیست؛ قسط نباید تسویه شده باشد.
        inst.IsPaid.Should().BeFalse();
    }

    [Fact]
    public async Task Cheque_Without_An_Image_Is_Rejected()
    {
        var inst = GivenInstallment();

        var act = () => _sut.SubmitChequeAsync(
            inst.Id, _employeeId,
            new SubmitChequeRequestDto { ChequeNumber = "1" },
            Array.Empty<byte>(), "x.jpg", "image/jpeg");

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Confirming_A_Cheque_Settles_The_Installment()
    {
        var inst = GivenInstallment();

        var payment = new InstallmentPayment
        {
            Id = Guid.NewGuid(),
            Method = PaymentMethod.Cheque,
            Status = InstallmentPaymentStatus.AwaitingAdminApproval,
            LoanInstallment = inst,
            LoanInstallmentId = inst.Id,
            EmployeeId = _employeeId,
        };

        _payments.Setup(x => x.GetByIdWithInstallmentAsync(payment.Id)).ReturnsAsync(payment);

        await _sut.ConfirmChequeAsync(payment.Id, Guid.NewGuid());

        inst.IsPaid.Should().BeTrue();
        inst.PaidMethod.Should().Be(PaymentMethod.Cheque);
        payment.Status.Should().Be(InstallmentPaymentStatus.Confirmed);
    }

    [Fact]
    public async Task Rejecting_A_Cheque_Leaves_The_Installment_Unpaid()
    {
        var inst = GivenInstallment();

        var payment = new InstallmentPayment
        {
            Id = Guid.NewGuid(),
            Method = PaymentMethod.Cheque,
            Status = InstallmentPaymentStatus.AwaitingAdminApproval,
            LoanInstallment = inst,
            LoanInstallmentId = inst.Id,
            EmployeeId = _employeeId,
        };

        _payments.Setup(x => x.GetByIdWithInstallmentAsync(payment.Id)).ReturnsAsync(payment);

        await _sut.RejectChequeAsync(payment.Id, Guid.NewGuid(), "چک مخدوش است");

        inst.IsPaid.Should().BeFalse();
        payment.Status.Should().Be(InstallmentPaymentStatus.Rejected);
        payment.RejectReason.Should().Be("چک مخدوش است");
    }

    [Fact]
    public async Task Rejecting_A_Cheque_Requires_A_Reason()
    {
        var act = () => _sut.RejectChequeAsync(Guid.NewGuid(), Guid.NewGuid(), "  ");

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    // ───────── درگاه ─────────

    [Fact]
    public async Task Gateway_Session_Has_An_Authority_And_Expiry()
    {
        var inst = GivenInstallment();

        var session = await _sut.StartGatewayPaymentAsync(inst.Id, _employeeId);

        session.Authority.Should().NotBeEmpty();
        session.Amount.Should().Be(inst.Amount);
        session.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        session.RedirectUrl.Should().Contain(session.Authority.ToString());
    }

    [Fact]
    public async Task Successful_Gateway_Payment_Settles_The_Installment()
    {
        var inst = GivenInstallment();
        var payment = await GivenGatewaySession(inst);

        var dto = await _sut.CompleteGatewayPaymentAsync(
            payment.GatewayAuthority!.Value, ValidCard());

        dto.Status.Should().Be("Confirmed");
        dto.GatewayRefId.Should().MatchRegex(@"^\d{12}$");
        inst.IsPaid.Should().BeTrue();
        inst.PaidMethod.Should().Be(PaymentMethod.OnlineGateway);
    }

    [Fact]
    public async Task Failed_Gateway_Payment_Leaves_The_Installment_Unpaid()
    {
        var inst = GivenInstallment();
        var payment = await GivenGatewaySession(inst);

        var card = ValidCard();
        card.SecondPassword = "12345"; // پنج رقم

        var act = () => _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, card);

        await act.Should().ThrowAsync<BusinessRuleException>();

        inst.IsPaid.Should().BeFalse();
        payment.Status.Should().Be(InstallmentPaymentStatus.Failed);
    }

    [Fact]
    public async Task Expired_Gateway_Session_Is_Rejected()
    {
        var inst = GivenInstallment();
        var payment = await GivenGatewaySession(inst);

        payment.GatewayExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        var act = () => _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, ValidCard());

        (await act.Should().ThrowAsync<BusinessRuleException>()).WithMessage("*مهلت*");
    }

    [Fact]
    public async Task Paying_The_Same_Session_Twice_Is_Rejected()
    {
        var inst = GivenInstallment();
        var payment = await GivenGatewaySession(inst);

        await _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, ValidCard());

        var act = () => _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, ValidCard());

        (await act.Should().ThrowAsync<BusinessRuleException>()).WithMessage("*قبلاً انجام شده*");
    }

    // ───────── بستن وام ─────────

    [Fact]
    public async Task Paying_The_Last_Installment_Closes_The_Loan()
    {
        // ۱۱ قسط از ۱۲ پرداخت شده؛ این آخری است.
        var inst = GivenInstallment(totalInstallments: 12, paidCount: 11);
        GivenOtherUnpaidInstallments(inst, false);

        var payment = await GivenGatewaySession(inst);

        await _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, ValidCard());

        inst.LoanRequest.Status.Should().Be(LoanStatus.Paid);
    }

    [Fact]
    public async Task Paying_A_Middle_Installment_Leaves_The_Loan_Open()
    {
        var inst = GivenInstallment(totalInstallments: 12, paidCount: 4);
        GivenOtherUnpaidInstallments(inst, true);

        var payment = await GivenGatewaySession(inst);

        await _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, ValidCard());

        inst.LoanRequest.Status.Should().Be(LoanStatus.Active);
    }

    [Fact]
    public async Task Loan_Is_Not_Closed_When_The_Navigation_Holds_Only_The_Paid_Installment()
    {
        // بازتولید دقیقِ چیزی که EF در عمل می‌سازد: قسط با Include بارگذاری شده،
        // پس مجموعه‌ی اقساطِ وام فقط همین یک قسط را دارد. اگر «همه پرداخت شدند»
        // از روی همین مجموعه حساب شود، وامِ ۱۲ قسطی با اولین پرداخت بسته می‌شود.
        var inst = GivenInstallment(totalInstallments: 12, paidCount: 0);

        inst.LoanRequest.LoanInstallments = new List<LoanInstallment> { inst };

        GivenOtherUnpaidInstallments(inst, true);

        var payment = await GivenGatewaySession(inst);

        await _sut.CompleteGatewayPaymentAsync(payment.GatewayAuthority!.Value, ValidCard());

        inst.LoanRequest.Status.Should().Be(LoanStatus.Active);
    }

    // ───────── قسط جاری ─────────

    [Fact]
    public async Task Current_Installment_Is_The_Earliest_Unpaid_One()
    {
        GivenInstallment(totalInstallments: 12, paidCount: 3);

        var dto = await _sut.GetCurrentInstallmentAsync(_employeeId);

        dto.HasDueInstallment.Should().BeTrue();
        dto.InstallmentNumber.Should().Be(4);
    }

    [Fact]
    public async Task No_Open_Loan_Means_No_Due_Installment()
    {
        _loans.Setup(x => x.GetOpenLoansWithInstallmentsAsync(_employeeId))
            .ReturnsAsync(new List<LoanRequest>());

        var dto = await _sut.GetCurrentInstallmentAsync(_employeeId);

        dto.HasDueInstallment.Should().BeFalse();
    }

    // ───────── کمکی ─────────

    private void GivenOtherUnpaidInstallments(LoanInstallment inst, bool any)
    {
        _installments
            .Setup(x => x.HasOtherUnpaidInstallmentsAsync(inst.LoanRequestId, inst.Id))
            .ReturnsAsync(any);
    }

    private async Task<InstallmentPayment> GivenGatewaySession(LoanInstallment inst)
    {
        await _sut.StartGatewayPaymentAsync(inst.Id, _employeeId);

        var payment = _added.Last();

        _payments.Setup(x => x.GetByAuthorityAsync(payment.GatewayAuthority!.Value))
            .ReturnsAsync(payment);

        payment.LoanInstallment = inst;

        return payment;
    }

    private static GatewayPaymentRequestDto ValidCard() => new()
    {
        CardNumber = "6037991234567890",
        Cvv2 = "123",
        ExpiryMonth = "08",
        ExpiryYear = "07",
        SecondPassword = "123456",
    };
}
