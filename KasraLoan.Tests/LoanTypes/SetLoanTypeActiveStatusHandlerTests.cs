using FluentAssertions;
using KasraLoan.Application.Features.LoanTypes.Commands.SetLoanTypeActiveStatus;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using Xunit;

namespace KasraLoan.Tests.LoanTypes;

public class SetLoanTypeActiveStatusHandlerTests
{
    private readonly Mock<ILoanTypeRepository> _loanTypeRepository;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly SetLoanTypeActiveStatusHandler _handler;

    public SetLoanTypeActiveStatusHandlerTests()
    {
        _loanTypeRepository = new Mock<ILoanTypeRepository>();
        _currentUserService = new Mock<ICurrentUserService>();

        // ادمین ارشد فرض می‌شود؛ به همه‌ی انواع وام دسترسی دارد.
        _currentUserService.Setup(x => x.CanManageLoanType(It.IsAny<int>())).Returns(true);

        _handler = new SetLoanTypeActiveStatusHandler(
            _loanTypeRepository.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task Handle_Should_Deactivate_LoanType()
    {
        var loanType = new LoanType
        {
            Id = 1,
            Name = "وام سفر",
            Type = LoanTypeEnum.TravelLoan,
            IsActive = true
        };

        _loanTypeRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(loanType);

        var result = await _handler.Handle(
            new SetLoanTypeActiveStatusCommand { LoanTypeId = 1, IsActive = false },
            CancellationToken.None);

        loanType.IsActive.Should().BeFalse();
        result.IsActive.Should().BeFalse();

        _loanTypeRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_LoanType_Not_Found()
    {
        _loanTypeRepository
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((LoanType?)null);

        Func<Task> action = async () =>
            await _handler.Handle(
                new SetLoanTypeActiveStatusCommand { LoanTypeId = 99, IsActive = false },
                CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();

        _loanTypeRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}
