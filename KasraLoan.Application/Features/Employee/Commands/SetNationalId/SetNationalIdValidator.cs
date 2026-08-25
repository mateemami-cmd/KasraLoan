using FluentValidation;
using KasraLoan.Application.Common;

namespace KasraLoan.Application.Features.Employee.Commands.SetNationalId
{
    public class SetNationalIdValidator : AbstractValidator<SetNationalIdCommand>
    {
        public SetNationalIdValidator()
        {
            RuleFor(x => x.Request.NationalId)
                .NotEmpty().WithMessage("کد ملی الزامی است.")
                .Must(NationalIdValidator.IsValid)
                    .WithMessage("کد ملی معتبر نیست (باید ۱۰ رقم با رقمِ کنترلیِ درست باشد).");
        }
    }
}
