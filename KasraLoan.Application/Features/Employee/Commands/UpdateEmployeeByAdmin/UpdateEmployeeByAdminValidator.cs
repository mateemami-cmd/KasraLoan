using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KasraLoan.Domain.Enums;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateEmployeeByAdmin
{
    public class UpdateEmployeeByAdminValidator : AbstractValidator<UpdateEmployeeByAdminCommand>
    {
        public UpdateEmployeeByAdminValidator()
        {
            RuleFor(x => x.Request.FirstName)
                .NotEmpty().WithMessage("نام الزامی است.")
                .MaximumLength(100);

            RuleFor(x => x.Request.LastName)
                .NotEmpty().WithMessage("نام‌خانوادگی الزامی است.")
                .MaximumLength(100);

            RuleFor(x => x.Request.PersonnelNumber)
                .NotEmpty().WithMessage("شماره پرسنلی الزامی است.")
                .MaximumLength(50);

            RuleFor(x => x.Request.Username)
                .NotEmpty().WithMessage("نام کاربری الزامی است.")
                .MinimumLength(4).WithMessage("نام کاربری باید حداقل ۴ کاراکتر باشد.")
                .Matches("^[a-zA-Z0-9._-]+$")
                    .WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، عدد، نقطه، خط تیره و آندرلاین باشد.");

            RuleFor(x => x.Request.HireDate)
                .NotEmpty().WithMessage("تاریخ استخدام الزامی است.");

            RuleFor(x => x.Request.Role)
                .Must(r => Enum.TryParse<UserRole>(r, ignoreCase: true, out _))
                .WithMessage("نقش واردشده معتبر نیست (باید Employee یا Admin باشد).");

            When(x => !string.IsNullOrWhiteSpace(x.Request.Email), () =>
            {
                RuleFor(x => x.Request.Email)
                    .EmailAddress()
                    .WithMessage("ایمیل واردشده معتبر نیست.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Request.PhoneNumber), () =>
            {
                RuleFor(x => x.Request.PhoneNumber)
                    .Matches(@"^09\d{9}$")
                    .WithMessage("شماره تماس باید یک شماره موبایل معتبر ایران باشد (مثال: 09123456789).");
            });
        }
    }
}