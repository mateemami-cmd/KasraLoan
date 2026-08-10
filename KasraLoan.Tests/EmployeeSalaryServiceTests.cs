using FluentAssertions;
using KasraLoan.Application.Services;
using KasraLoan.Domain.Entities;
using Xunit;

namespace KasraLoan.Tests
{
    public class EmployeeSalaryServiceTests
    {
        private readonly EmployeeSalaryService _sut = new();

        [Fact]
        public void Uses_Job_Position_Base_Salary_When_No_Override()
        {
            var employee = new Employee
            {
                JobPosition = new JobPosition { Title = "دواپس", BaseSalary = 60_000_000 }
            };

            _sut.GetEffectiveMonthlySalary(employee).Should().Be(60_000_000);
        }

        [Fact]
        public void Employee_Specific_Salary_Overrides_Position_Base_Salary()
        {
            var employee = new Employee
            {
                MonthlySalary = 70_000_000,
                JobPosition = new JobPosition { Title = "دواپس", BaseSalary = 60_000_000 }
            };

            _sut.GetEffectiveMonthlySalary(employee).Should().Be(70_000_000);
        }

        [Fact]
        public void Returns_Zero_When_Neither_Salary_Nor_Position_Exists()
        {
            _sut.GetEffectiveMonthlySalary(new Employee()).Should().Be(0);
        }

        [Fact]
        public void Max_Monthly_Installment_Is_One_Third_Of_Salary()
        {
            var employee = new Employee { MonthlySalary = 60_000_000 };

            // ۳۳٫۳۳٪ از ۶۰ میلیون
            _sut.GetMaxMonthlyInstallment(employee).Should().Be(19_998_000);
        }

        [Fact]
        public void Max_Monthly_Installment_Is_Zero_Without_Salary()
        {
            _sut.GetMaxMonthlyInstallment(new Employee()).Should().Be(0);
        }
    }
}
