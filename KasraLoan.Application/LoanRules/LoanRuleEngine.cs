using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace KasraLoan.Application.LoanRules
{
    public class LoanRuleEngine : ILoanRuleEngine
    {
        private readonly IEnumerable<ILoanRule> _rules;
        private readonly IEmployeeScoreService _employeeScoreService;
        private readonly IEmployeeSalaryService _employeeSalaryService;
        private readonly ILoanCalculationService _loanCalculationService;

        public LoanRuleEngine(
            IEnumerable<ILoanRule> rules,
            IEmployeeScoreService employeeScoreService,
            IEmployeeSalaryService employeeSalaryService,
            ILoanCalculationService loanCalculationService)
        {
            _rules = rules;
            _employeeScoreService = employeeScoreService;
            _employeeSalaryService = employeeSalaryService;
            _loanCalculationService = loanCalculationService;
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

                var result = rule.Evaluate(context);

                return ApplySalaryCap(context, result);
            }

            return new LoanRuleResult
            {
                IsAllowed = false,
                Message = "هیچ قانون فعالی برای این نوع وام یافت نشد."
            };
        }

        /// <summary>
        /// گیت نسبت قسط به حقوق (DTI). بعد از قانون هر نوع وام اعمال می‌شود تا همه‌ی
        /// قوانین — و هر قانونی که بعداً اضافه شود — به‌صورت خودکار مشمولش باشند.
        ///
        /// خروجی این گیت است که باعث می‌شود سقف وامِ یک دواپس با یک کارمند پشتیبانی
        /// فرق کند، بدون این‌که هیچ قانون مخصوصِ سمت شغلی وجود داشته باشد.
        /// </summary>
        private LoanRuleResult ApplySalaryCap(LoanRuleContext context, LoanRuleResult result)
        {
            if (!result.IsAllowed)
                return result;

            var maxMonthlyInstallment =
                _employeeSalaryService.GetMaxMonthlyInstallment(context.Employee);

            if (maxMonthlyInstallment <= 0)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message =
                        "حقوق ماهانه‌ی شما در سیستم ثبت نشده است. " +
                        "برای بررسی درخواست وام، ابتدا باید سمت شغلی یا حقوق شما توسط ادمین ثبت شود.",
                    MaxAllowedAmount = 0,
                    MaxInstallments = result.MaxInstallments,
                    AnnualFeePercent = result.AnnualFeePercent
                };
            }

            // تعداد اقساطی که واقعاً اعمال می‌شود، همانی است که هندلر هم استفاده می‌کند:
            // درخواست کارمند، ولی هرگز بیشتر از سقف مجاز آن نوع وام.
            var effectiveInstallmentCount = context.RequestedInstallmentCount > 0
                ? Math.Min(context.RequestedInstallmentCount, result.MaxInstallments)
                : result.MaxInstallments;

            var salaryCap = _loanCalculationService.CalculateMaxPrincipalForMonthlyCap(
                maxMonthlyInstallment,
                result.AnnualFeePercent,
                effectiveInstallmentCount);

            if (salaryCap < result.MaxAllowedAmount)
                result.MaxAllowedAmount = salaryCap;

            if (context.RequestedAmount > result.MaxAllowedAmount)
            {
                result.IsAllowed = false;
                result.Message =
                    $"با حقوق فعلی شما، سقف قسط ماهانه {maxMonthlyInstallment:N0} تومان است " +
                    $"و در {effectiveInstallmentCount} قسط حداکثر می‌توانید " +
                    $"{result.MaxAllowedAmount:N0} تومان وام بگیرید. " +
                    "می‌توانید مبلغ کمتری درخواست دهید یا تعداد اقساط را بیشتر کنید.";
            }

            return result;
        }
    }
}
