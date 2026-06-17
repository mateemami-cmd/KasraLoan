using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules.Implementations
{
    public class MarriageLoanRule : ILoanRule
    {
        public bool CanApply(LoanRuleContext context)
        {
            return context.LoanType.Name == "Marriage Loan";
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            var maxAmount = 200_000_000;

            if (context.RequestedAmount > maxAmount)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message = "سقف وام ازدواج 200 میلیون تومان است.",
                    MaxAllowedAmount = maxAmount
                };
            }

            return new LoanRuleResult
            {
                IsAllowed = true,
                Message = "OK",
                MaxAllowedAmount = maxAmount,
                MaxInstallments = 24,
                MonthlyFeePercent = 5
            };
        }
    }
}