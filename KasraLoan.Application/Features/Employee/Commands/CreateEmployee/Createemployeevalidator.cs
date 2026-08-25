using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KasraLoan.Domain.Enums;

namespace KasraLoan.Application.Features.Employee.Commands.CreateEmployee
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.Request.FirstName)
                .NotEmpty().WithMessage("نام الزامی است.")
                .MaximumLength(100);

            RuleFor(x => x.Request.LastName)
                .NotEmpty().WithMessage("نام‌خانوادگی الزامی است.")
                .MaximumLength(100);

            // کد ملی برای همه (کارمند و ادمین) الزامی و دقیقاً ۱۰ رقم است؛
            // در «فراموشی رمز عبور» برای احراز هویت استفاده می‌شود.
            RuleFor(x => x.Request.NationalId)
                .NotEmpty().WithMessage("کد ملی الزامی است.")
                .Matches("^[0-9]{10}$").WithMessage("کد ملی باید دقیقاً ۱۰ رقم باشد.");

            // رمز را ادمین هنگام ساخت تعیین می‌کند (دیگر رمز خودکار نداریم).
            RuleFor(x => x.Request.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.");

            // نام کاربری و شماره‌ی پرسنلی کارمند خودکار (و یکسان) ساخته می‌شوند؛
            // فقط برای ادمین دستی و الزامی‌اند.
            When(x => IsAdmin(x.Request.Role), () =>
            {
                RuleFor(x => x.Request.PersonnelNumber)
                    .NotEmpty().WithMessage("شماره پرسنلی الزامی است.")
                    .Matches("^[0-9]+$").WithMessage("شماره پرسنلی فقط می‌تواند شامل عدد باشد.")
                    .MaximumLength(50);

                RuleFor(x => x.Request.Username)
                    .NotEmpty().WithMessage("نام کاربری الزامی است.")
                    .MinimumLength(4).WithMessage("نام کاربری باید حداقل ۴ کاراکتر باشد.")
                    .Matches("^[a-zA-Z0-9._-]+$")
                        .WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، عدد، نقطه، خط تیره و آندرلاین باشد.");
            });

            RuleFor(x => x.Request.HireDate)
                .NotEmpty().WithMessage("تاریخ استخدام الزامی است.");

            When(x => !string.IsNullOrWhiteSpace(x.Request.Role), () =>
            {
                RuleFor(x => x.Request.Role)
                    .Must(r => Enum.TryParse<UserRole>(r, ignoreCase: true, out _))
                    .WithMessage("نقش واردشده معتبر نیست (باید Employee یا Admin باشد).");
            });

            // سمت شغلی فقط برای کارمند اجباری است: حقوق — و در نتیجه سقف قسط وام —
            // از روی آن حساب می‌شود. ادمین می‌تواند بدون سمت ثبت شود.
            When(x => !IsAdmin(x.Request.Role), () =>
            {
                RuleFor(x => x.Request.JobPositionId)
                    .NotNull().WithMessage("انتخاب سمت شغلی برای کارمند الزامی است.")
                    .GreaterThan(0).WithMessage("سمت شغلی انتخاب‌شده معتبر نیست.");
            });

            When(x => x.Request.MonthlySalary.HasValue, () =>
            {
                RuleFor(x => x.Request.MonthlySalary!.Value)
                    .GreaterThan(0).WithMessage("حقوق ماهانه باید بزرگ‌تر از صفر باشد.");
            });
        }

        private static bool IsAdmin(string? role)
        {
            return Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed)
                && parsed == UserRole.Admin;
        }
    }
}