using KasraLoan.Application.Interfaces.Services;
using System.Collections.Generic;

namespace KasraLoan.Application.LoanRules
{
    public class LoanRuleEngine : ILoanRuleEngine
    {
        private readonly IEnumerable<ILoanRule> _rules;
        private readonly IEmployeeScoreService _employeeScoreService;

        public LoanRuleEngine(IEnumerable<ILoanRule> rules, IEmployeeScoreService employeeScoreService)
        {
            _rules = rules;
            _employeeScoreService = employeeScoreService;
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            if (context.EmployeeScore < _employeeScoreService.MinimumScoreRequiredForLoan)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message =
                        $"امتیاز شما ({context.EmployeeScore}) کمتر از حداقل امتیاز لازم " +
                        $"({_employeeScoreService.MinimumScoreRequiredForLoan}) برای دریافت وام است."
                };
            }

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