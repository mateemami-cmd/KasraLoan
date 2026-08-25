using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ResetByIdentity
{
    /// <summary>مرحله‌ی اولِ فراموشیِ رمز: تأییدِ نام کاربری + کد ملی.</summary>
    public class VerifyIdentityCommand : IRequest<VerifyIdentityResponse>
    {
        public VerifyIdentityRequestDto Request { get; set; } = null!;
    }

    public class VerifyIdentityResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
