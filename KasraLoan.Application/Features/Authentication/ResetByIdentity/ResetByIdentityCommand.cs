using KasraLoan.Application.Common.Logging;
using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ResetByIdentity
{
    // ISensitiveRequest: رمزِ خام دارد و نباید در لاگ نوشته شود.
    public class ResetByIdentityCommand : IRequest<ResetByIdentityResponse>, ISensitiveRequest
    {
        public ResetByIdentityRequestDto Request { get; set; } = null!;
    }

    public class ResetByIdentityResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
