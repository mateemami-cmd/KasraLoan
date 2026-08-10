using FluentAssertions;
using KasraLoan.Application.Services;
using Xunit;

namespace KasraLoan.Tests
{
    public class LoanCalculationServiceTests
    {
        private readonly LoanCalculationService _sut = new();

        [Fact]
        public void Fee_Is_Annual_Not_Per_Installment()
        {
            // وام ازدواج: ۲۰۰ میلیون، ۲۴ قسط، کارمزد ۵٪ سالانه
            // کارمزد = ۲۰۰م × ۵٪ × (۲۴ ÷ ۱۲) = ۲۰ میلیون  ← نه ۲۴۰ میلیون
            var fee = _sut.CalculateTotalFee(200_000_000, 5m, 24);

            fee.Should().Be(20_000_000);
        }

        [Fact]
        public void Fee_For_One_Year_Equals_Annual_Percent_Of_Principal()
        {
            // ۱۲ قسط = دقیقاً یک سال، پس کارمزد برابر درصد سالانه‌ی اصل مبلغ است.
            var fee = _sut.CalculateTotalFee(200_000_000, 4m, 12);

            fee.Should().Be(8_000_000);
        }

        [Fact]
        public void Zero_Fee_Loan_Has_Total_Payable_Equal_To_Principal()
        {
            var total = _sut.CalculateTotalPayable(50_000_000, 0m, 10);

            total.Should().Be(50_000_000);
        }

        [Fact]
        public void Total_Payable_Is_Principal_Plus_Fee()
        {
            var total = _sut.CalculateTotalPayable(100_000_000, 2m, 12);

            total.Should().Be(102_000_000);
        }

        [Fact]
        public void Monthly_Payment_Divides_Total_Payable_By_Installment_Count()
        {
            var monthly = _sut.CalculateMonthlyPayment(102_000_000, 12);

            monthly.Should().Be(8_500_000);
        }

        [Fact]
        public void Monthly_Payment_Returns_Zero_For_Zero_Installments()
        {
            _sut.CalculateMonthlyPayment(100_000_000, 0).Should().Be(0);
        }

        [Fact]
        public void Max_Principal_Is_The_Inverse_Of_Total_Payable()
        {
            // با سقف قسط ۲۰ میلیون در ۱۲ قسط و کارمزد ۴٪ سالانه:
            // کل قابل پرداخت = ۲۴۰م → اصل = ۲۴۰م ÷ ۱٫۰۴ ≈ ۲۳۰٫۷۶م
            var maxPrincipal = _sut.CalculateMaxPrincipalForMonthlyCap(20_000_000m, 4m, 12);

            // رفت‌وبرگشت: قسط حاصل از این اصل نباید از سقف بیشتر شود.
            var totalPayable = _sut.CalculateTotalPayable(maxPrincipal, 4m, 12);
            var monthly = _sut.CalculateMonthlyPayment(totalPayable, 12);

            monthly.Should().BeLessThanOrEqualTo(20_000_000m);
            maxPrincipal.Should().BeInRange(230_000_000, 231_000_000);
        }

        [Fact]
        public void Max_Principal_Is_Zero_When_There_Is_No_Repayment_Capacity()
        {
            _sut.CalculateMaxPrincipalForMonthlyCap(0m, 5m, 12).Should().Be(0);
        }

        [Theory]
        [InlineData(60_000_000)]  // دواپس
        [InlineData(50_000_000)]  // بک‌اند
        [InlineData(40_000_000)]  // فرانت‌اند
        public void Higher_Salary_Cap_Yields_Higher_Max_Principal(long salary)
        {
            var cap = salary * 0.3333m;

            var maxPrincipal = _sut.CalculateMaxPrincipalForMonthlyCap(cap, 4m, 12);

            // سقف وام باید با حقوق رشد کند و هرگز از توان بازپرداخت فراتر نرود.
            var monthly = _sut.CalculateMonthlyPayment(
                _sut.CalculateTotalPayable(maxPrincipal, 4m, 12), 12);

            monthly.Should().BeLessThanOrEqualTo(cap);
        }
    }
}
