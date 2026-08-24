using FluentValidation;

namespace KasraLoan.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.");
        }
    }
}
