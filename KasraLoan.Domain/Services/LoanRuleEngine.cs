using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Services
{
    public class LoanRuleEngine : ILoanRuleEngine
    {
        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            return new LoanRuleResult
            {
                IsAllowed = context.EmployeeScore >= 600,
                MonthlyFeePercent = 3
            };
        }
    }
}