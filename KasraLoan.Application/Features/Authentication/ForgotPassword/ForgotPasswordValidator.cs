using FluentValidation;

namespace KasraLoan.Application.Features.Authentication.ForgotPassword
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("ایمیل را وارد کنید.")
                .EmailAddress().WithMessage("فرمتِ ایمیل درست نیست.");
        }
    }
}
