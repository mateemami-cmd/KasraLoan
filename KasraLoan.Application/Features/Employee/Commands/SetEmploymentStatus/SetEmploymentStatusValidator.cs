using FluentValidation;
using KasraLoan.Domain.Enums;
using System;

namespace KasraLoan.Application.Features.Employee.Commands.SetEmploymentStatus
{
    public class SetEmploymentStatusValidator : AbstractValidator<SetEmploymentStatusCommand>
    {
        public SetEmploymentStatusValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.Request.Status)
                .NotEmpty().WithMessage("وضعیت اشتغال الزامی است.")
                .Must(s => Enum.TryParse<EmploymentStatus>(s, ignoreCase: true, out _))
                .WithMessage("وضعیت واردشده معتبر نیست (باید Active یا Terminated باشد).");

            RuleFor(x => x.Request.Reason)
                .NotEmpty().WithMessage("ثبت دلیل تغییر وضعیت الزامی است.")
                .MaximumLength(500).WithMessage("دلیل حداکثر ۵۰۰ کاراکتر است.");
        }
    }
}
