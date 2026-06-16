using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules.Implementations
{
    public class TravelLoanRule : ILoanRule
    {
        public bool CanApply(LoanRuleContext context)
        {
            return context.LoanTypeEnum == LoanTypeEnum.TravelLoan;
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            var maxAllowedAmount =
                (context.EmployeeScore * 10_000) + 10_000_000;

            if (context.RequestedAmount > maxAllowedAmount)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message = "مبلغ درخواستی بیشتر از سقف وام سفر است.",
                    MaxAllowedAmount = maxAllowedAmount
                };
            }

            return new LoanRuleResult
            {
                IsAllowed = true,
                Message = "OK",
                MaxAllowedAmount = maxAllowedAmount,
                MaxInstallments = 10,
                MonthlyFeePercent = 0
            };
        }
    }
}