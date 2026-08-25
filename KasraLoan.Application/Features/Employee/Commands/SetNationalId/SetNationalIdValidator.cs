using FluentValidation;

namespace KasraLoan.Application.Features.Employee.Commands.SetNationalId
{
    public class SetNationalIdValidator : AbstractValidator<SetNationalIdCommand>
    {
        public SetNationalIdValidator()
        {
            RuleFor(x => x.Request.NationalId)
                .NotEmpty().WithMessage("کد ملی الزامی است.")
                .Matches("^[0-9]{10}$").WithMessage("کد ملی باید دقیقاً ۱۰ رقم باشد.");
        }
    }
}
