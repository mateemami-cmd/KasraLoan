using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.RejectLoan
{
    public class RejectLoanCommand : IRequest<RejectLoanResponse>
    {
        public Guid LoanRequestId { get; set; }

        /// <summary>
        /// دلیل رد. اختیاری است ولی وقتی نوشته شود هم در وام ذخیره می‌شود و هم
        /// در اعلانِ کارمند می‌آید — پیام «رد شد» بدون دلیل برای کارمند بی‌فایده است.
        /// </summary>
        public string? RejectReason { get; set; }
    }
}