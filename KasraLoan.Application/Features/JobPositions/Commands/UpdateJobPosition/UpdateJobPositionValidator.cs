using FluentValidation;

namespace KasraLoan.Application.Features.JobPositions.Commands.UpdateJobPosition
{
    public class UpdateJobPositionValidator : AbstractValidator<UpdateJobPositionCommand>
    {
        public UpdateJobPositionValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Request.Title)
                .NotEmpty().WithMessage("عنوان سمت شغلی الزامی است.")
                .MaximumLength(100).WithMessage("عنوان سمت شغلی حداکثر ۱۰۰ کاراکتر است.");

            RuleFor(x => x.Request.BaseSalary)
                .GreaterThan(0).WithMessage("حقوق پایه باید بزرگ‌تر از صفر باشد.");
        }
    }
}
