using FluentAssertions;
using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Features.Authentication.Login;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using Moq;
using Xunit;

namespace KasraLoan.Tests.Authentication;

public class LoginHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<IJwtService> _jwtService;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository;

    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _employeeRepository = new Mock<IEmployeeRepository>();

        _passwordHasher = new Mock<IPasswordHasher>();

        _jwtService = new Mock<IJwtService>();

        _refreshTokenRepository = new Mock<IRefreshTokenRepository>();

        _handler = new LoginHandler(
            _employeeRepository.Object,
            _passwordHasher.Object,
            _jwtService.Object,
            _refreshTokenRepository.Object);
    }

    [Fact]
    public async Task Handle_Should_Login_Successfully()
    {
        // Arrange

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Matin",
            PersonnelNumber = "1001",
            PasswordHash = "HASH",
            IsActive = true
        };

        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "matin",
                Password = "123456"
            }
        };

        _employeeRepository
            .Setup(x => x.GetByUsernameAsync(command.LoginRequest.Username))
            .ReturnsAsync(employee);

        _passwordHasher
            .Setup(x => x.Verify(command.LoginRequest.Password, employee.PasswordHash))
            .Returns(true);

        _jwtService
            .Setup(x => x.GenerateToken(
                employee.Id,
                employee.FirstName,
                employee.PersonnelNumber,
                employee.Role.ToString()))
            .Returns("ACCESS_TOKEN");

        _jwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("REFRESH_TOKEN");

        // Act

        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert

        result.Should().NotBeNull();

        result.AccessToken.Should().Be("ACCESS_TOKEN");

        result.RefreshToken.Should().Be("REFRESH_TOKEN");

        _refreshTokenRepository.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "wrong",
                Password = "123"
            }
        };

        _employeeRepository
            .Setup(x => x.GetByUsernameAsync(command.LoginRequest.Username))
            .ReturnsAsync((Employee?)null);

        Func<Task> action = async () =>
            await _handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Is_Inactive()
    {
        var employee = new Employee
        {
            IsActive = false
        };

        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "matin",
                Password = "123"
            }
        };

        _employeeRepository
            .Setup(x => x.GetByUsernameAsync(command.LoginRequest.Username))
            .ReturnsAsync(employee);

        Func<Task> action = async () =>
            await _handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Password_Is_Wrong()
    {
        var employee = new Employee
        {
            PasswordHash = "HASH",
            IsActive = true
        };

        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "matin",
                Password = "wrong"
            }
        };

        _employeeRepository
            .Setup(x => x.GetByUsernameAsync(command.LoginRequest.Username))
            .ReturnsAsync(employee);

        _passwordHasher
            .Setup(x => x.Verify(command.LoginRequest.Password, employee.PasswordHash))
            .Returns(false);

        Func<Task> action = async () =>
            await _handler.Handle(command, CancellationToken.None);

        await action.Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }
}