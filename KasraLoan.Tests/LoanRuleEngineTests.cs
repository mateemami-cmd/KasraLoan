using FluentAssertions;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace KasraLoan.Tests
{
    public class LoanRuleEngineTests
    {
        private static LoanRuleContext CreateContext(int employeeScore)
        {
            return new LoanRuleContext
            {
                Employee = new Employee { Id = System.Guid.NewGuid() },
                LoanType = new LoanType { Type = LoanTypeEnum.MarriageLoan },
                RequestedAmount = 50_000_000,
                EmployeeScore = employeeScore
            };
        }

        [Fact]
        public void Should_Reject_Loan_When_Score_Is_Below_Minimum()
        {
            var scoreServiceMock = new Mock<IEmployeeScoreService>();
            scoreServiceMock.Setup(x => x.MinimumScoreRequiredForLoan).Returns(600);

            var alwaysApplicableRule = new Mock<ILoanRule>();
            alwaysApplicableRule.Setup(x => x.CanApply(It.IsAny<LoanRuleContext>())).Returns(true);
            alwaysApplicableRule.Setup(x => x.Evaluate(It.IsAny<LoanRuleContext>()))
                .Returns(new LoanRuleResult { IsAllowed = true, MaxAllowedAmount = 100_000_000, MaxInstallments = 12 });

            var engine = new LoanRuleEngine(
                new List<ILoanRule> { alwaysApplicableRule.Object },
                scoreServiceMock.Object);

            var context = CreateContext(employeeScore: 500);

            var result = engine.Evaluate(context);

            result.IsAllowed.Should().BeFalse();
        }

        [Fact]
        public void Should_Allow_Loan_When_Score_Meets_Minimum_And_Rule_Applies()
        {
            var scoreServiceMock = new Mock<IEmployeeScoreService>();
            scoreServiceMock.Setup(x => x.MinimumScoreRequiredForLoan).Returns(600);

            var alwaysApplicableRule = new Mock<ILoanRule>();
            alwaysApplicableRule.Setup(x => x.CanApply(It.IsAny<LoanRuleContext>())).Returns(true);
            alwaysApplicableRule.Setup(x => x.Evaluate(It.IsAny<LoanRuleContext>()))
                .Returns(new LoanRuleResult { IsAllowed = true, MaxAllowedAmount = 100_000_000, MaxInstallments = 12 });

            var engine = new LoanRuleEngine(
                new List<ILoanRule> { alwaysApplicableRule.Object },
                scoreServiceMock.Object);

            var context = CreateContext(employeeScore: 600);

            var result = engine.Evaluate(context);

            result.IsAllowed.Should().BeTrue();
        }

        [Fact]
        public void Should_Reject_Loan_When_No_Matching_Rule_Found()
        {
            var scoreServiceMock = new Mock<IEmployeeScoreService>();
            scoreServiceMock.Setup(x => x.MinimumScoreRequiredForLoan).Returns(600);

            var neverApplicableRule = new Mock<ILoanRule>();
            neverApplicableRule.Setup(x => x.CanApply(It.IsAny<LoanRuleContext>())).Returns(false);

            var engine = new LoanRuleEngine(
                new List<ILoanRule> { neverApplicableRule.Object },
                scoreServiceMock.Object);

            var context = CreateContext(employeeScore: 1000);

            var result = engine.Evaluate(context);

            result.IsAllowed.Should().BeFalse();
        }
    }
}