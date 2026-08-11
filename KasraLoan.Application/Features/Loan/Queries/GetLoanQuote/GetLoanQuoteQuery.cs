using KasraLoan.Application.DTOs.Loans;
using MediatR;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanQuote
{
    public class GetLoanQuoteQuery : IRequest<LoanQuoteDto>
    {
        public int LoanTypeId { get; set; }

        /// <summary>
        /// مبلغ انتخابی کارمند. اگر داده شود، گزینه‌های تعداد اقساط هم با قسط
        /// ماهانه‌ی هرکدام برگردانده می‌شوند.
        /// </summary>
        public long? Amount { get; set; }
    }
}
