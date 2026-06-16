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
        public decimal MonthlyFeePercent { get; set; }
    }
}