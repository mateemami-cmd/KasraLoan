using FluentAssertions;
using KasraLoan.Application.Features.LoanTypes.Queries.GetAllLoanTypes;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using Xunit;

namespace KasraLoan.Tests.LoanTypes;

public class GetAllLoanTypesHandlerTests
{
    private readonly Mock<ILoanTypeRepository> _loanTypeRepository;
    private readonly GetAllLoanTypesHandler _handler;

    public GetAllLoanTypesHandlerTests()
    {
        _loanTypeRepository = new Mock<ILoanTypeRepository>();
        _handler = new GetAllLoanTypesHandler(_loanTypeRepository.Object);
    }

    private void SetupLoanTypes() =>
        _loanTypeRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<LoanType>
            {
                new() { Id = 1, Name = "وام سفر", Type = LoanTypeEnum.TravelLoan, IsActive = true },
                new() { Id = 2, Name = "وام قرض‌الحسنه", Type = LoanTypeEnum.QarzolhasanehLoan, IsActive = false },
                new() { Id = 4, Name = "وام ازدواج", Type = LoanTypeEnum.MarriageLoan, IsActive = true }
            });

    [Fact]
    public async Task Handle_Should_Return_All_LoanTypes_When_ActiveOnly_False()
    {
        SetupLoanTypes();

        var result = await _handler.Handle(
            new GetAllLoanTypesQuery { ActiveOnly = false },
            CancellationToken.None);

        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Active_LoanTypes_When_ActiveOnly_True()
    {
        SetupLoanTypes();

        var result = await _handler.Handle(
            new GetAllLoanTypesQuery { ActiveOnly = true },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.IsActive);
    }
}
