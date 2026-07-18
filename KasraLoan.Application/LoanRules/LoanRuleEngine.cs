using System.Collections.Generic;

namespace KasraLoan.Application.LoanRules
{
    public class LoanRuleEngine : ILoanRuleEngine
    {
        private readonly IEnumerable<ILoanRule> _rules;

        public LoanRuleEngine(IEnumerable<ILoanRule> rules)
        {
            _rules = rules;
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            foreach (var rule in _rules)
            {
                if (!rule.CanApply(context))
                    continue;

                return rule.Evaluate(context);
            }

            return new LoanRuleResult
            {
                IsAllowed = false,
                Message = "هیچ قانون فعالی برای این نوع وام یافت نشد."
            };
        }
    }
}