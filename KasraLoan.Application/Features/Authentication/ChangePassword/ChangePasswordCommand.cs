using KasraLoan.Application.Common.Logging;
using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ChangePassword
{
    // ISensitiveRequest: رمزهای خام دارد و نباید در لاگ نوشته شود.
    public class ChangePasswordCommand : IRequest<ChangePasswordResponse>, ISensitiveRequest
    {
        public ChangePasswordRequestDto Request { get; set; } = null!;
    }

    public class ChangePasswordResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
