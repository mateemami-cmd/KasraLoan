using FluentValidation;

namespace KasraLoan.Application.Features.Authentication.ChangePassword
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.Request.CurrentPassword)
                .NotEmpty().WithMessage("رمز عبور فعلی الزامی است.");

            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.");
        }
    }
}
