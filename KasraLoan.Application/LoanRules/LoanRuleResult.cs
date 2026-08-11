using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules
{
    public class LoanRuleResult
    {
        public bool IsAllowed { get; set; }
        public string? Message { get; set; }
        public decimal MaxAllowedAmount { get; set; }
        public int MaxInstallments { get; set; }
        /// <summary>کارمزد صندوق به‌صورت درصد <b>سالانه</b> روی اصل مبلغ.</summary>
        public decimal AnnualFeePercent { get; set; }

        /// <summary>
        /// آیا این نوع وام برای تأیید، مدرک پشتیبان لازم دارد.
        /// در قانون هر نوع وام تعریف می‌شود، نه در فرم — تا ادمین نتواند وامی را
        /// که مدرک می‌خواهد بدون مدرک تأیید کند.
        /// </summary>
        public bool RequiresDocument { get; set; }

        /// <summary>
        /// چه مدرکی لازم است؛ متنش هم به کارمند در فرم نشان داده می‌شود و هم
        /// در پیام خطای تأیید به ادمین.
        /// </summary>
        public string? RequiredDocumentDescription { get; set; }
    }
}