using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.LoanRules
{
    public interface ILoanRule
    {
        bool CanApply(LoanRuleContext context);
        LoanRuleResult Evaluate(LoanRuleContext context);
    }
}