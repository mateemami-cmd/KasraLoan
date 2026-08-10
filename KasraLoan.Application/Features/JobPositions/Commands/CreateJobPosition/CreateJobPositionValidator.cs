using FluentValidation;

namespace KasraLoan.Application.Features.JobPositions.Commands.CreateJobPosition
{
    public class CreateJobPositionValidator : AbstractValidator<CreateJobPositionCommand>
    {
        public CreateJobPositionValidator()
        {
            RuleFor(x => x.Request.Title)
                .NotEmpty().WithMessage("عنوان سمت شغلی الزامی است.")
                .MaximumLength(100).WithMessage("عنوان سمت شغلی حداکثر ۱۰۰ کاراکتر است.");

            RuleFor(x => x.Request.BaseSalary)
                .GreaterThan(0).WithMessage("حقوق پایه باید بزرگ‌تر از صفر باشد.");
        }
    }
}
