using System.Collections.Generic;
using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.GetLoginHistory
{
    /// <summary>سه ورودِ اخیرِ کاربرِ جاری، برای «تاریخچه ورودهای اخیر».</summary>
    public class GetLoginHistoryQuery : IRequest<GetLoginHistoryResponse>
    {
    }

    public class GetLoginHistoryResponse
    {
        public List<LoginHistoryDto> History { get; set; } = new();
    }
}
