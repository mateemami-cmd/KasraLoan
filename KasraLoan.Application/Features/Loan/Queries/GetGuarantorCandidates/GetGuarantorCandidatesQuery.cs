using System.Collections.Generic;
using MediatR;

namespace KasraLoan.Application.Features.Loan.Queries.GetGuarantorCandidates
{
    /// <summary>
    /// فهرستِ کارمندانی که می‌توانند ضامنِ وام شوند: فقط کارمندانِ فعال (نه ادمین،
    /// نه غیرفعال/حذف‌شده) و به‌جز خودِ درخواست‌دهنده. برای سرچ در فرمِ وام ازدواج.
    /// </summary>
    public class GetGuarantorCandidatesQuery : IRequest<List<GuarantorCandidateDto>>
    {
        /// <summary>متنِ جست‌وجو روی نام/نام‌خانوادگی/شماره‌ی پرسنلی (اختیاری).</summary>
        public string? Search { get; set; }
    }

    public class GuarantorCandidateDto
    {
        public System.Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PersonnelNumber { get; set; } = string.Empty;
    }
}
