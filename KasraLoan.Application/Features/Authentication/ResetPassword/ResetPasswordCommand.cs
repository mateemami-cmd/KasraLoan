using KasraLoan.Application.Common.Logging;
using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ResetPassword
{
    // ISensitiveRequest: رمزِ خام دارد و نباید در لاگ نوشته شود.
    public class ResetPasswordCommand : IRequest<ResetPasswordResponse>, ISensitiveRequest
    {
        public ResetPasswordRequestDto Request { get; set; } = null!;
    }

    public class ResetPasswordResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
