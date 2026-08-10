using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules
{
    public class LoanRuleContext
    {
        public Employee Employee { get; set; }

        public LoanType LoanType { get; set; }

        public string LoanTypeName => LoanType.Name;

        public decimal RequestedAmount { get; set; }

        public int EmployeeScore { get; set; }

        /// <summary>
        /// تعداد اقساط درخواستی کارمند. برای گیت DTI لازم است، چون سقف مبلغ
        /// از روی حقوق فقط وقتی قابل محاسبه است که بدانیم در چند قسط پرداخت می‌شود.
        /// </summary>
        public int RequestedInstallmentCount { get; set; }
    }
}