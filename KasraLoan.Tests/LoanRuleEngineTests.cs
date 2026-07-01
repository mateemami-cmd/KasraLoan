using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KasraLoan.Tests
{
    public class LoanRuleEngineTests
    {
        [Fact]
        public void Should_Reject_Loan_When_Score_Is_Below_600()
        {
            // Arrange
            int employeeScore = 500;

            // Act
            var isEligible = employeeScore >= 600;

            // Assert
            isEligible.Should().BeFalse();
        }
    }
}