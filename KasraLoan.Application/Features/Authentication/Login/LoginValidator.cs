using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Login
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.LoginRequest.Username)
                .NotEmpty().WithMessage("نام کاربری الزامی است.");

            RuleFor(x => x.LoginRequest.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است.");
        }
    }
}