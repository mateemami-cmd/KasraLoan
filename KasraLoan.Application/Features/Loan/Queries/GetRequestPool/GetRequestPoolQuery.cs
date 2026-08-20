using System.Collections.Generic;
using KasraLoan.Application.DTOs.Loans;
using MediatR;

namespace KasraLoan.Application.Features.Loan.Queries.GetRequestPool
{
    /// <summary>
    /// «استخرِ درخواست‌ها»: همه‌ی درخواست‌های ثبت‌شده‌ی کارمندان (وام + مجوز وام)
    /// یکجا. مخصوص ادمین ارشد است؛ ادمین‌های وام نمای فیلترشده‌ی خودشان را دارند.
    /// </summary>
    public class GetRequestPoolQuery : IRequest<GetRequestPoolResponse>
    {
    }

    public class GetRequestPoolResponse
    {
        public List<RequestPoolItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
