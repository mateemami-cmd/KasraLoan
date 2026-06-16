using System.Collections.Generic;

namespace KasraLoan.Application.LoanRules
{
    public class LoanRuleEngine
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

                var result = rule.Evaluate(context);

                // اگر رد شد همونجا قطع کن
                if (!result.IsAllowed)
                    return result;

                // چون فقط یک Rule باید مسئول باشد
                return result;
            }

            return new LoanRuleResult
            {
                IsAllowed = false,
                Message = "هیچ قانون فعالی برای این نوع وام یافت نشد."
            };
        }
    }
}