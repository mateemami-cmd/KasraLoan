using FluentAssertions;
using FluentValidation.TestHelper;
using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Features.Authentication.Login;
using Xunit;

namespace KasraLoan.Tests.Authentication;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "",
                Password = "123456"
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LoginRequest.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "matin",
                Password = ""
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LoginRequest.Password);
    }

    [Fact]
    public void Should_Not_Have_Validation_Error()
    {
        var command = new LoginCommand
        {
            LoginRequest = new LoginRequestDto
            {
                Username = "matin",
                Password = "123456"
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}