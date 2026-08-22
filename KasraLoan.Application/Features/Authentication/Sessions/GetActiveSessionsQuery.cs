using System.Collections.Generic;
using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.Sessions
{
    /// <summary>نشست‌های فعالِ کاربرِ جاری، برای صفحه‌ی «نشست‌های فعال».</summary>
    public class GetActiveSessionsQuery : IRequest<GetActiveSessionsResponse>
    {
    }

    public class GetActiveSessionsResponse
    {
        public List<SessionDto> Sessions { get; set; } = new();
    }
}
