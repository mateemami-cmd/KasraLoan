using System;
using System.Collections.Generic;

namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// تقویم شمسی و پنجره‌های چرخه‌ی حقوق.
    /// همه‌ی تصمیم‌های «الان داخل پنجره هستیم یا نه» باید از اینجا بگذرد، نه از
    /// محاسبه‌ی دستی روی <see cref="DateTime"/>.
    /// </summary>
    public interface IPayrollCalendarService
    {
        /// <summary>لحظه‌ی جاری به وقت ایران (نه UTC).</summary>
        DateTime NowInIran();

        /// <summary>روز ماه شمسی برای یک لحظه‌ی UTC.</summary>
        int GetPersianDayOfMonth(DateTime utc);

        /// <summary>تعداد روزهای ماه شمسیِ یک لحظه‌ی UTC (۲۹ تا ۳۱).</summary>
        int GetDaysInPersianMonth(DateTime utc);

        /// <summary>تاریخ شمسی به شکل «۱۴۰۵/۰۵/۱۹» برای نمایش در پیام‌ها.</summary>
        string ToPersianDateString(DateTime utc);

        /// <summary>
        /// آیا الان داخل پنجره‌ی مجاز تغییر وضعیت اشتغال هستیم؟
        /// پنجره دور ماه می‌پیچد (مثلاً از ۲۸ام تا ۱ام ماه بعد).
        /// </summary>
        bool IsWithinEmploymentChangeWindow(DateTime utc);

        /// <summary>آیا الان داخل پنجره‌ی انتخاب روش پرداخت قسط هستیم؟</summary>
        bool IsWithinPaymentMethodSelectionWindow(DateTime utc);

        /// <summary>توضیح متنی پنجره‌ی تغییر وضعیت اشتغال، برای پیام خطا.</summary>
        string DescribeEmploymentChangeWindow();

        /// <summary>
        /// سررسید اقساط یک وام: روزِ پرداخت حقوق در ماه‌های شمسیِ پیاپی.
        ///
        /// سررسیدها عمداً به چرخه‌ی حقوق گره خورده‌اند نه به تاریخ تأیید وام —
        /// چون قسط از حقوق کسر می‌شود، و پنجره‌ی انتخاب روش پرداخت هم حول همان
        /// تاریخ تعریف شده است.
        /// </summary>
        /// <param name="approvedAtUtc">لحظه‌ی تأیید وام.</param>
        /// <param name="count">تعداد اقساط.</param>
        List<DateTime> GetInstallmentDueDatesUtc(DateTime approvedAtUtc, int count);
    }
}
