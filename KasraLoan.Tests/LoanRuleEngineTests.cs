using FluentAssertions;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.Services;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace KasraLoan.Tests
{
    public class LoanRuleEngineTests
    {
        private const long DevOpsSalary = 60_000_000;

        private static LoanRuleContext CreateContext(
            int employeeScore,
            long monthlySalary = DevOpsSalary,
            decimal requestedAmount = 50_000_000,
            int requestedInstallmentCount = 12)
        {
            return new LoanRuleContext
            {
                Employee = new Employee
                {
                    Id = System.Guid.NewGuid(),
                    MonthlySalary = monthlySalary
                },
                LoanType = new LoanType { Type = LoanTypeEnum.MarriageLoan },
                RequestedAmount = requestedAmount,
                EmployeeScore = employeeScore,
                RequestedInstallmentCount = requestedInstallmentCount
            };
        }

        private static LoanRuleEngine CreateEngine(params ILoanRule[] rules)
        {
            var scoreServiceMock = new Mock<IEmployeeScoreService>();
            scoreServiceMock.Setup(x => x.MinimumScoreRequiredForLoan).Returns(600);

            // سرویس‌های حقوق و محاسبه واقعی‌اند نه mock، چون منطق گیت DTI دقیقاً
            // همان چیزی است که این تست‌ها باید پوشش بدهند.
            return new LoanRuleEngine(
                new List<ILoanRule>(rules),
                scoreServiceMock.Object,
                new EmployeeSalaryService(),
                new LoanCalculationService());
        }

        private static ILoanRule AlwaysApplicableRule(
            long maxAllowedAmount = 100_000_000,
            int maxInstallments = 12,
            decimal annualFeePercent = 0)
        {
            var rule = new Mock<ILoanRule>();

            rule.Setup(x => x.CanApply(It.IsAny<LoanRuleContext>())).Returns(true);
            rule.Setup(x => x.Evaluate(It.IsAny<LoanRuleContext>()))
                .Returns(() => new LoanRuleResult
                {
                    IsAllowed = true,
                    MaxAllowedAmount = maxAllowedAmount,
                    MaxInstallments = maxInstallments,
                    AnnualFeePercent = annualFeePercent
                });

            return rule.Object;
        }

        [Fact]
        public void Should_Reject_Loan_When_Score_Is_Below_Minimum()
        {
            var engine = CreateEngine(AlwaysApplicableRule());

            var result = engine.Evaluate(CreateContext(employeeScore: 500));

            result.IsAllowed.Should().BeFalse();
        }

        [Fact]
        public void Should_Allow_Loan_When_Score_Meets_Minimum_And_Rule_Applies()
        {
            var engine = CreateEngine(AlwaysApplicableRule());

            var result = engine.Evaluate(CreateContext(employeeScore: 600));

            result.IsAllowed.Should().BeTrue();
        }

        [Fact]
        public void Should_Reject_Loan_When_No_Matching_Rule_Found()
        {
            var neverApplicableRule = new Mock<ILoanRule>();
            neverApplicableRule.Setup(x => x.CanApply(It.IsAny<LoanRuleContext>())).Returns(false);

            var engine = CreateEngine(neverApplicableRule.Object);

            var result = engine.Evaluate(CreateContext(employeeScore: 1000));

            result.IsAllowed.Should().BeFalse();
        }

        [Fact]
        public void Should_Reject_Loan_When_Employee_Has_No_Salary_Registered()
        {
            var engine = CreateEngine(AlwaysApplicableRule());

            var context = CreateContext(employeeScore: 1000, monthlySalary: 0);
            context.Employee.MonthlySalary = null;
            context.Employee.JobPosition = null;

            var result = engine.Evaluate(context);

            result.IsAllowed.Should().BeFalse();
            result.Message.Should().Contain("حقوق");
        }

        [Fact]
        public void Should_Cap_Max_Amount_By_Salary_When_Rule_Cap_Is_Higher()
        {
            // فرانت‌اند: حقوق ۴۰ م → سقف قسط ۱۳٫۳۳ م → در ۱۲ قسط با کارمزد صفر
            // حداکثر حدود ۱۵۹٫۹ م، که از سقف ۲۰۰ میلیونی قانون کمتر است.
            var engine = CreateEngine(AlwaysApplicableRule(maxAllowedAmount: 200_000_000));

            var result = engine.Evaluate(CreateContext(
                employeeScore: 1000,
                monthlySalary: 40_000_000,
                requestedAmount: 100_000_000,
                requestedInstallmentCount: 12));

            result.IsAllowed.Should().BeTrue();
            result.MaxAllowedAmount.Should().BeLessThan(200_000_000);
            result.MaxAllowedAmount.Should().BeGreaterThan(150_000_000);
        }

        [Fact]
        public void Should_Reject_When_Requested_Amount_Exceeds_Salary_Capacity()
        {
            var engine = CreateEngine(AlwaysApplicableRule(maxAllowedAmount: 200_000_000));

            var result = engine.Evaluate(CreateContext(
                employeeScore: 1000,
                monthlySalary: 40_000_000,
                requestedAmount: 190_000_000,
                requestedInstallmentCount: 12));

            result.IsAllowed.Should().BeFalse();
            result.Message.Should().Contain("سقف قسط ماهانه");
        }

        [Fact]
        public void Higher_Salary_Should_Allow_Higher_Loan_For_Same_Rule()
        {
            var engine = CreateEngine(AlwaysApplicableRule(maxAllowedAmount: 500_000_000));

            var devOps = engine.Evaluate(CreateContext(
                employeeScore: 1000, monthlySalary: 60_000_000, requestedAmount: 1));

            var frontend = engine.Evaluate(CreateContext(
                employeeScore: 1000, monthlySalary: 40_000_000, requestedAmount: 1));

            // تفکیک بین سمت‌ها بدون هیچ قانون مخصوص سمت شغلی
            devOps.MaxAllowedAmount.Should().BeGreaterThan(frontend.MaxAllowedAmount);
        }

        [Fact]
        public void More_Installments_Should_Allow_Higher_Loan_For_Same_Salary()
        {
            var engine = CreateEngine(AlwaysApplicableRule(
                maxAllowedAmount: 500_000_000, maxInstallments: 24));

            var shortTerm = engine.Evaluate(CreateContext(
                employeeScore: 1000, requestedAmount: 1, requestedInstallmentCount: 12));

            var longTerm = engine.Evaluate(CreateContext(
                employeeScore: 1000, requestedAmount: 1, requestedInstallmentCount: 24));

            longTerm.MaxAllowedAmount.Should().BeGreaterThan(shortTerm.MaxAllowedAmount);
        }
    }
}
