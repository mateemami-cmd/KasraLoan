using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules.Implementations
{
    public class ImmediatePaymentLoanRule : ILoanRule
    {
        public bool CanApply(LoanRuleContext context)
        {
            return context.LoanType.Name == "وام پرداخت فوری";
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            Console.WriteLine(">>> ImmediatePaymentLoanRule is running");

            return new LoanRuleResult
            {
                IsAllowed = true,
                Message = "OK",
                MaxAllowedAmount = 100_000_000,
                MaxInstallments = 12,
                MonthlyFeePercent = 2
            };
        }
    }
}