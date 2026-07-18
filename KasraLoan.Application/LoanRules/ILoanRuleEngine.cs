using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules
{
    public interface ILoanRuleEngine
    {
        LoanRuleResult Evaluate(LoanRuleContext context);
    }
}