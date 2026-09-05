using FluentValidation;
using System.Linq;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateProfile
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.Request)
                .Must(r =>
                    !string.IsNullOrWhiteSpace(r.NewPassword) ||
                    !string.IsNullOrWhiteSpace(r.PhoneNumber) ||
                    r.AdditionalPhoneNumbers != null ||
                    !string.IsNullOrWhiteSpace(r.Email) ||
                    r.AdditionalEmails != null)
                .WithMessage("حداقل یکی از فیلدها (رمز عبور، شماره تماس، شماره‌های اضافه، ایمیل) باید ارسال شود.");

            When(x => !string.IsNullOrWhiteSpace(x.Request.NewPassword), () =>
            {
                RuleFor(x => x.Request.NewPassword)
                    .MinimumLength(8)
                        .WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
                    .Must(p => p!.Any(char.IsDigit))
                        .WithMessage("رمز عبور باید حداقل شامل یک عدد باشد.")
                    .Must(p => p!.Any(char.IsLetter))
                        .WithMessage("رمز عبور باید حداقل شامل یک حرف باشد.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Request.PhoneNumber), () =>
            {
                RuleFor(x => x.Request.PhoneNumber)
                    .Matches(@"^09\d{9}$")
                    .WithMessage("شماره تماس باید یک شماره موبایل معتبر ایران باشد (مثال: 09123456789).");
            });

            RuleForEach(x => x.Request.AdditionalPhoneNumbers)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Matches(@"^09\d{9}$")
                .WithMessage("هر شماره تماس باید یک شماره موبایل معتبر ایران باشد (مثال: 09123456789).");

            When(x => !string.IsNullOrWhiteSpace(x.Request.Email), () =>
            {
                RuleFor(x => x.Request.Email)
                    .EmailAddress()
                    .WithMessage("ایمیل واردشده معتبر نیست.");
            });

            RuleForEach(x => x.Request.AdditionalEmails)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .EmailAddress()
                .WithMessage("هر ایمیل واردشده باید معتبر باشد.");
        }
    }
}