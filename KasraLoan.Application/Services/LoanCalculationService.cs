using KasraLoan.Application.Interfaces.Services;
using System;

namespace KasraLoan.Application.Services
{
    /// <inheritdoc cref="ILoanCalculationService"/>
    public class LoanCalculationService : ILoanCalculationService
    {
        private const int MonthsPerYear = 12;

        public long CalculateTotalFee(long principal, decimal annualFeePercent, int installmentCount)
        {
            if (principal <= 0 || annualFeePercent <= 0 || installmentCount <= 0)
                return 0;

            var fee = principal
                * (annualFeePercent / 100m)
                * ((decimal)installmentCount / MonthsPerYear);

            return (long)Math.Round(fee, MidpointRounding.AwayFromZero);
        }

        public long CalculateTotalPayable(long principal, decimal annualFeePercent, int installmentCount)
        {
            return principal + CalculateTotalFee(principal, annualFeePercent, installmentCount);
        }

        public decimal CalculateMonthlyPayment(long totalPayable, int installmentCount)
        {
            if (installmentCount <= 0)
                return 0;

            return Math.Round((decimal)totalPayable / installmentCount, 0, MidpointRounding.AwayFromZero);
        }

        public long CalculateMaxPrincipalForMonthlyCap(
            decimal maxMonthlyInstallment,
            decimal annualFeePercent,
            int installmentCount)
        {
            if (maxMonthlyInstallment <= 0 || installmentCount <= 0)
                return 0;

            // سقف کل چیزی که در طول دوره می‌تواند بپردازد
            var maxTotalPayable = maxMonthlyInstallment * installmentCount;

            // مبلغ کل = اصل × (۱ + نرخ سالانه × سال‌های دوره)
            // پس: اصل = مبلغ کل ÷ (۱ + نرخ سالانه × سال‌های دوره)
            var feeMultiplier = 1m
                + ((annualFeePercent / 100m) * ((decimal)installmentCount / MonthsPerYear));

            var maxPrincipal = maxTotalPayable / feeMultiplier;

            // رو به پایین گرد می‌شود تا سقف هرگز از توان بازپرداخت فراتر نرود.
            return (long)Math.Floor(maxPrincipal);
        }
    }
}
