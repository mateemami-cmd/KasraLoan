using MediatR;

namespace KasraLoan.Application.Features.Authentication.Sessions
{
    /// <summary>یک نشستِ فعالِ کاربرِ جاری را قطع (باطل) می‌کند.</summary>
    public class RevokeSessionCommand : IRequest<RevokeSessionResponse>
    {
        public int SessionId { get; set; }
    }

    public class RevokeSessionResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
