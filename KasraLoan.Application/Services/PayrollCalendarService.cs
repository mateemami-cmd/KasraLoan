using KasraLoan.Application.Common.Payroll;
using KasraLoan.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace KasraLoan.Application.Services
{
    /// <inheritdoc cref="IPayrollCalendarService"/>
    public class PayrollCalendarService : IPayrollCalendarService
    {
        // مرز روزها باید به وقت ایران باشد نه UTC: ساعت ۲۱:۰۰ UTC روز ۲۷ام،
        // در تهران از قبل ۰۰:۳۰ روز ۲۸ام است. اگر با UTC حساب کنیم، پنجره‌ها
        // ۳ ساعت و نیم دیر باز و دیر بسته می‌شوند.
        private static readonly TimeSpan IranOffset = TimeSpan.FromMinutes(210); // +03:30

        private static readonly PersianCalendar Persian = new();

        private readonly PayrollCycleOptions _options;

        public PayrollCalendarService(IOptions<PayrollCycleOptions> options)
        {
            _options = options.Value;
        }

        public DateTime NowInIran() => ToIranTime(DateTime.UtcNow);

        public int GetPersianDayOfMonth(DateTime utc)
        {
            return Persian.GetDayOfMonth(ToIranTime(utc));
        }

        public int GetDaysInPersianMonth(DateTime utc)
        {
            var iranTime = ToIranTime(utc);

            return Persian.GetDaysInMonth(
                Persian.GetYear(iranTime),
                Persian.GetMonth(iranTime));
        }

        public string ToPersianDateString(DateTime utc)
        {
            var iranTime = ToIranTime(utc);

            return $"{Persian.GetYear(iranTime):0000}/" +
                   $"{Persian.GetMonth(iranTime):00}/" +
                   $"{Persian.GetDayOfMonth(iranTime):00}";
        }

        public bool IsWithinEmploymentChangeWindow(DateTime utc)
        {
            var day = GetPersianDayOfMonth(utc);

            return IsInWrappingWindow(
                day,
                _options.EmploymentChangeOpenDay,
                _options.EmploymentChangeCloseDay);
        }

        public bool IsWithinPaymentMethodSelectionWindow(DateTime utc)
        {
            // این پنجره نمی‌پیچد: از روز مشخصی باز می‌شود و تا پایان همان ماه
            // شمسی باز می‌ماند — چه ماه ۲۹ روزه باشد چه ۳۱ روزه.
            return GetPersianDayOfMonth(utc) >= _options.PaymentMethodSelectionOpenDay;
        }

        public string DescribeEmploymentChangeWindow()
        {
            return $"از روز {_options.EmploymentChangeOpenDay} هر ماه شمسی " +
                   $"تا روز {_options.EmploymentChangeCloseDay} ماه بعد";
        }

        public List<DateTime> GetInstallmentDueDatesUtc(DateTime approvedAtUtc, int count)
        {
            var dates = new List<DateTime>();

            if (count <= 0)
                return dates;

            var approvedIran = ToIranTime(approvedAtUtc);

            var year = Persian.GetYear(approvedIran);
            var month = Persian.GetMonth(approvedIran);

            // از روزِ حقوقِ همین ماه شمسی شروع می‌کنیم و آن‌قدر جلو می‌رویم تا
            // فاصله‌ی کافی از تاریخ تأیید داشته باشیم.
            var first = PayrollDateInIran(year, month);

            while ((first - approvedIran).TotalDays < _options.MinDaysToFirstInstallment)
            {
                (year, month) = NextPersianMonth(year, month);
                first = PayrollDateInIran(year, month);
            }

            for (var i = 0; i < count; i++)
            {
                dates.Add(ToUtc(PayrollDateInIran(year, month)));

                (year, month) = NextPersianMonth(year, month);
            }

            return dates;
        }

        /// <summary>
        /// روزِ پرداخت حقوق در یک ماه شمسی مشخص، به وقت ایران و ساعت ۰۰:۰۰.
        /// اگر آن روز در ماه وجود نداشته باشد (مثلاً روز ۳۱ در ماهی ۳۰ روزه)،
        /// به آخرین روز ماه محدود می‌شود.
        /// </summary>
        private DateTime PayrollDateInIran(int persianYear, int persianMonth)
        {
            var daysInMonth = Persian.GetDaysInMonth(persianYear, persianMonth);

            var day = Math.Min(_options.PayrollRunDay, daysInMonth);

            return Persian.ToDateTime(persianYear, persianMonth, day, 0, 0, 0, 0);
        }

        private static (int Year, int Month) NextPersianMonth(int year, int month)
        {
            return month == 12 ? (year + 1, 1) : (year, month + 1);
        }

        private static DateTime ToUtc(DateTime iranTime)
        {
            return DateTime.SpecifyKind(iranTime - IranOffset, DateTimeKind.Utc);
        }

        private static DateTime ToIranTime(DateTime utc)
        {
            var asUtc = utc.Kind == DateTimeKind.Utc
                ? utc
                : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            return asUtc + IranOffset;
        }

        /// <summary>
        /// پنجره‌ای که از انتهای یک ماه شروع و در ابتدای ماه بعد تمام می‌شود.
        /// مثلاً از ۲۸ تا ۱: روزهای ۲۸، ۲۹، (۳۰)، (۳۱) و ۱ داخل پنجره‌اند.
        /// چون ماه‌های شمسی ۲۹ تا ۳۱ روزه‌اند، طول پنجره خودبه‌خود متفاوت می‌شود
        /// و لازم نیست جایی به تعداد روزهای ماه اشاره کنیم.
        /// </summary>
        private static bool IsInWrappingWindow(int day, int openDay, int closeDay)
        {
            if (openDay <= closeDay)
                return day >= openDay && day <= closeDay;

            return day >= openDay || day <= closeDay;
        }
    }
}
