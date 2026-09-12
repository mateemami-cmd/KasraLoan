using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.LoanRules.Implementations
{
    public class MarriageLoanRule : ILoanRule
    {
        /// <summary>مهلت درخواست وام ازدواج پس از تاریخ عقد: ۶ ماه.</summary>
        public const int RequestDeadlineMonths = 6;

        /// <summary>حداقل سابقه‌ی کار لازم برای وام ازدواج: ۱ سال.</summary>
        public const int MinTenureMonths = 12;

        // مبلغ وام ازدواج = مبلغ پایه + (امتیازِ صندوقِ قرض‌الحسنه × ۳۰۰ × ۱٫۴)،
        // با سقفِ نهایی. «امتیازِ صندوقِ قرض‌الحسنه» = تعداد ماه‌های سابقه × ۱۰۰۰.
        private const long BaseAmount = 120_000_000;
        private const long MaxAmount = 150_000_000;

        public bool CanApply(LoanRuleContext context)
        {
            return context.LoanType.Type == LoanTypeEnum.MarriageLoan;
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            // برای همه‌ی مسیرها یک سقفِ منطقی برمی‌گردانیم تا فرم چیزی برای نمایش داشته باشد.
            var now = DateTime.UtcNow.Date;

            // تاریخ عقد مشخصه‌ی کارمند است و از پروفایل او خوانده می‌شود.
            var marriageDate = context.Employee?.MarriageDate;

            if (marriageDate == null)
            {
                return Deny(
                    "تاریخ عقد شما در سیستم ثبت نشده است. " +
                    "برای درخواست وام ازدواج، ابتدا آن را در فرم وارد کنید.");
            }

            if (marriageDate.Value.Date > now)
                return Deny("تاریخ عقد ثبت‌شده در آینده است و معتبر نیست.");

            // مهلت ۶ ماهه از تاریخِ ثبتِ ازدواج (تاریخ عقد).
            if (marriageDate.Value.Date.AddMonths(RequestDeadlineMonths) < now)
            {
                return Deny(
                    $"مهلت درخواست وام ازدواج، {RequestDeadlineMonths} ماه پس از تاریخ عقد است " +
                    "و این مهلت گذشته است.");
            }

            // حداقل ۱ سال سابقه‌ی کار در گروه کسرا.
            var tenureMonths = MonthsBetween(context.Employee?.HireDate, now);
            if (tenureMonths < MinTenureMonths)
            {
                return Deny(
                    "برای دریافت وام ازدواج باید حداقل ۱ سال سابقه‌ی کار در گروه کسرا داشته باشید.");
            }

            // مبلغِ وامِ قابلِ دریافت از روی سابقه:
            // امتیاز = ماه‌های سابقه × ۱۰۰۰ ؛ مبلغ = پایه + امتیاز × ۳۰۰ × ۱٫۴ ؛ با سقفِ ۱۵۰م.
            long score = tenureMonths * 1000L;
            long variable = (long)Math.Round(score * 300m * 1.40m);
            long amount = Math.Min(MaxAmount, BaseAmount + variable);

            if (context.RequestedAmount > amount)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message =
                        $"سقف وام ازدواجِ شما بر اساس سابقه {amount:N0} تومان است.",
                    MaxAllowedAmount = amount
                };
            }

            return new LoanRuleResult
            {
                IsAllowed = true,
                Message = "OK",
                MaxAllowedAmount = amount,
                MaxInstallments = 20,
                AnnualFeePercent = 2,
                RequiresDocument = true,
                RequiredDocumentDescription =
                    "تصویر صفحه‌ی اولِ شناسنامه و صفحه‌ی اولِ سند ازدواج"
            };
        }

        private static LoanRuleResult Deny(string message) => new()
        {
            IsAllowed = false,
            Message = message,
            MaxAllowedAmount = MaxAmount
        };

        /// <summary>تعداد ماه‌های کاملِ سپری‌شده بین دو تاریخ (میلادی، تقریبِ کافی).</summary>
        private static int MonthsBetween(DateTime? from, DateTime to)
        {
            if (from == null) return 0;
            var f = from.Value.Date;
            var months = ((to.Year - f.Year) * 12) + to.Month - f.Month;
            if (to.Day < f.Day) months--;
            return Math.Max(0, months);
        }
    }
}