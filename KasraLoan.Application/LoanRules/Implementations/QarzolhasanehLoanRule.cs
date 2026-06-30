using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.LoanRules;
using KasraLoan.Domain.Enums;

namespace KasraLoan.Application.LoanRules.Implementations
{
    public class QarzolhasanehLoanRule : ILoanRule
    {
        public bool CanApply(LoanRuleContext context)
        {
            return context.LoanType.Type == LoanTypeEnum.QarzolhasanehLoan;
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            return new LoanRuleResult
            {
                IsAllowed = false,
                Message = "این وام در حال حاضر فعال نمی‌باشد."
            };
        }
    }
}