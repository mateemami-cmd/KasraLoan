using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestResponse
    {
        public Guid LoanRequestId { get; set; }

        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// آیا بعد از ثبت، باید مدرک هم بارگذاری شود.
        /// چون اندپوینت آپلود به شناسه‌ی وام نیاز دارد، مدرک فقط بعد از ساخته
        /// شدن درخواست قابل ارسال است؛ فرم با همین فیلد مرحله‌ی دوم را باز می‌کند.
        /// </summary>
        public bool RequiresDocument { get; set; }

        public string? RequiredDocumentDescription { get; set; }
    }
}