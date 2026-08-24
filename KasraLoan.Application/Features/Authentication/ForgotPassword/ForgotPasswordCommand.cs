using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
    {
        public ForgotPasswordRequestDto Request { get; set; } = null!;
    }

    public class ForgotPasswordResponse
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>آیا ایمیلِ واقعی ارسال شد (SMTP فعال بود).</summary>
        public bool EmailSent { get; set; }

        /// <summary>
        /// فقط در حالتِ توسعه (SMTP خاموش): رمزِ موقت اینجا برمی‌گردد تا بدونِ ایمیلِ
        /// واقعی هم بشود جریان را تست کرد. وقتی ایمیلِ واقعی برود، null است.
        /// </summary>
        public string? DevTempPassword { get; set; }
    }
}
