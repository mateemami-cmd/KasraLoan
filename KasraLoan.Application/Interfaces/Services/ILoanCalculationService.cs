namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// تنها مرجع محاسبات پولی وام. هر جایی که لازم است کارمزد، مبلغ کل قابل بازپرداخت
    /// یا مبلغ قسط حساب شود باید از این سرویس استفاده کند تا فرمول‌ها از هم جدا نیفتند.
    ///
    /// مدل کارمزد: کارمزد <b>ساده و سالانه</b> روی اصل مبلغ (نه مانده‌ی کاهشی).
    ///     کارمزد کل = اصل × (درصد سالانه ÷ ۱۰۰) × (تعداد اقساط ÷ ۱۲)
    /// </summary>
    public interface ILoanCalculationService
    {
        /// <summary>کارمزد کل وام (به تومان، گردشده).</summary>
        long CalculateTotalFee(long principal, decimal annualFeePercent, int installmentCount);

        /// <summary>مبلغ کل قابل بازپرداخت = اصل + کارمزد کل.</summary>
        long CalculateTotalPayable(long principal, decimal annualFeePercent, int installmentCount);

        /// <summary>مبلغ قسط ماهانه = مبلغ کل قابل بازپرداخت ÷ تعداد اقساط.</summary>
        decimal CalculateMonthlyPayment(long totalPayable, int installmentCount);

        /// <summary>
        /// عکسِ محاسبه‌ی بالا: با داشتن سقف قسط ماهانه، بیشترین اصل مبلغی که
        /// کارمند می‌تواند بگیرد چقدر است. مبنای گیت DTI (نسبت قسط به حقوق).
        /// </summary>
        long CalculateMaxPrincipalForMonthlyCap(
            decimal maxMonthlyInstallment,
            decimal annualFeePercent,
            int installmentCount);
    }
}
