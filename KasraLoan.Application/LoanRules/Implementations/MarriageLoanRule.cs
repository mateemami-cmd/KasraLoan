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
        /// <summary>
        /// مهلت درخواست وام ازدواج پس از تاریخ عقد، به ماه.
        ///
        /// صفر یعنی بدون محدودیت — پیش‌فرض عمداً همین است، چون کارمندانی که
        /// تاریخ عقدشان قدیمی است نباید ناگهان از این وام محروم شوند. برای
        /// فعال کردن قانون، کافی است این عدد را مثلاً روی ۱۲ بگذارید.
        /// </summary>
        public const int RequestDeadlineMonths = 0;

        public bool CanApply(LoanRuleContext context)
        {
            return context.LoanType.Type == LoanTypeEnum.MarriageLoan;
        }

        public LoanRuleResult Evaluate(LoanRuleContext context)
        {
            var maxAmount = 200_000_000;

            // تاریخ عقد مشخصه‌ی کارمند است و از پروفایل او خوانده می‌شود.
            // تا امروز این فیلد ذخیره می‌شد ولی هیچ‌جا بررسی نمی‌شد، یعنی کسی
            // که اصلاً ازدواج نکرده هم می‌توانست وام ازدواج بگیرد.
            var marriageDate = context.Employee?.MarriageDate;

            if (marriageDate == null)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message =
                        "تاریخ عقد شما در سیستم ثبت نشده است. " +
                        "برای درخواست وام ازدواج، ابتدا آن را در فرم وارد کنید.",
                    MaxAllowedAmount = maxAmount
                };
            }

            if (marriageDate.Value.Date > DateTime.UtcNow.Date)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message = "تاریخ عقد ثبت‌شده در آینده است و معتبر نیست.",
                    MaxAllowedAmount = maxAmount
                };
            }

            if (RequestDeadlineMonths > 0
                && marriageDate.Value.Date.AddMonths(RequestDeadlineMonths) < DateTime.UtcNow.Date)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message =
                        $"مهلت درخواست وام ازدواج، {RequestDeadlineMonths} ماه پس از تاریخ عقد است " +
                        "و این مهلت گذشته است.",
                    MaxAllowedAmount = maxAmount
                };
            }

            if (context.RequestedAmount > maxAmount)
            {
                return new LoanRuleResult
                {
                    IsAllowed = false,
                    Message = "سقف وام ازدواج 200 میلیون تومان است.",
                    MaxAllowedAmount = maxAmount
                };
            }

            return new LoanRuleResult
            {
                IsAllowed = true,
                Message = "OK",
                MaxAllowedAmount = maxAmount,
                MaxInstallments = 24,
                AnnualFeePercent = 5,
                RequiresDocument = true,
                RequiredDocumentDescription = "تصویر سند ازدواج"
            };
        }
    }
}