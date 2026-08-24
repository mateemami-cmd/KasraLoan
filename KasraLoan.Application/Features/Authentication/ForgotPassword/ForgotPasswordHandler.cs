using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ForgotPassword
{
    public class ForgotPasswordHandler
        : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordHandler(
            IEmployeeRepository employeeRepository,
            IPasswordHasher passwordHasher,
            IEmailSender emailSender)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
        }

        public async Task<ForgotPasswordResponse> Handle(
            ForgotPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Request.Email?.Trim() ?? string.Empty;

            var employee = await _employeeRepository.GetByEmailAsync(email);

            // طبقِ خواسته: اگر ایمیل درست/ثبت‌شده نبود، صریح به کاربر می‌گوییم.
            if (employee == null)
                throw new BusinessRuleException("این ایمیل در سیستم ثبت نشده است.");

            if (!employee.IsActive)
                throw new BusinessRuleException("حساب کاربریِ این ایمیل غیرفعال است.");

            // رمزِ موقت می‌سازیم، هش‌شده ذخیره می‌کنیم و فلگِ «باید عوض شود» را می‌زنیم.
            var tempPassword = GenerateTempPassword();
            employee.PasswordHash = _passwordHasher.Hash(tempPassword);
            employee.MustResetPassword = true;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            var sent = await _emailSender.SendAsync(
                employee.Email!,
                "بازیابیِ رمز عبور — صندوق همیار کسرا",
                BuildEmailBody(employee.FirstName, tempPassword));

            return new ForgotPasswordResponse
            {
                EmailSent = sent,
                Message = sent
                    ? "رمزِ موقت به ایمیل شما ارسال شد. با آن وارد شوید و رمزِ جدید بگذارید."
                    : "رمزِ موقت ساخته شد (حالتِ تست: ایمیل ارسال نشد).",
                // فقط وقتی ایمیلِ واقعی نرفت، رمز را برمی‌گردانیم تا قابلِ تست باشد.
                DevTempPassword = sent ? null : tempPassword
            };
        }

        // رمزِ موقتِ ۱۰ کاراکتری از حروف و اعداد، بدونِ کاراکترهای گیج‌کننده (O/0/l/1/I).
        private static string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(10);
            var result = new char[10];
            for (int i = 0; i < result.Length; i++)
                result[i] = chars[bytes[i] % chars.Length];
            return new string(result);
        }

        private static string BuildEmailBody(string firstName, string tempPassword) => $@"
<div dir=""rtl"" style=""font-family:Tahoma,Arial,sans-serif;font-size:14px;color:#222;line-height:2"">
  <p>سلام {firstName} عزیز،</p>
  <p>درخواستِ بازیابیِ رمز عبور برای حساب شما ثبت شد. رمزِ موقتِ شما:</p>
  <p style=""font-size:20px;font-weight:bold;letter-spacing:2px;background:#f2f4f8;padding:12px 16px;border-radius:8px;display:inline-block;direction:ltr"">{tempPassword}</p>
  <p>با این رمز وارد شوید؛ بلافاصله از شما خواسته می‌شود یک رمزِ جدیدِ دلخواه بگذارید.</p>
  <p style=""color:#888;font-size:12px"">توجه: رمزِ قبلیِ شما با همین رمزِ موقت جایگزین شد. اگر این درخواست را شما نداده‌اید، سریعاً با مدیر تماس بگیرید.</p>
</div>";
    }
}
