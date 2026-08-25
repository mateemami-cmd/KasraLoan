using FluentValidation;

namespace KasraLoan.Application.Features.Authentication.ResetByIdentity
{
    public class VerifyIdentityValidator : AbstractValidator<VerifyIdentityCommand>
    {
        public VerifyIdentityValidator()
        {
            RuleFor(x => x.Request.Username).NotEmpty().WithMessage("نام کاربری را وارد کنید.");
            RuleFor(x => x.Request.NationalId)
                .NotEmpty().WithMessage("کد ملی را وارد کنید.")
                .Matches("^[0-9]{10}$").WithMessage("کد ملی باید دقیقاً ۱۰ رقم باشد.");
        }
    }

    public class ResetByIdentityValidator : AbstractValidator<ResetByIdentityCommand>
    {
        public ResetByIdentityValidator()
        {
            RuleFor(x => x.Request.Username).NotEmpty().WithMessage("نام کاربری را وارد کنید.");
            RuleFor(x => x.Request.NationalId)
                .NotEmpty().WithMessage("کد ملی را وارد کنید.")
                .Matches("^[0-9]{10}$").WithMessage("کد ملی باید دقیقاً ۱۰ رقم باشد.");
            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید را وارد کنید.")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.");
        }
    }
}
