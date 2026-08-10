using FluentAssertions;
using KasraLoan.Application.Common.Payroll;
using KasraLoan.Application.Services;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using Xunit;

namespace KasraLoan.Tests
{
    public class PayrollCalendarServiceTests
    {
        private static readonly PersianCalendar Persian = new();

        private static PayrollCalendarService CreateSut()
        {
            return new PayrollCalendarService(
                Options.Create(new PayrollCycleOptions()));
        }

        /// <summary>
        /// یک لحظه‌ی UTC می‌سازد که در تهران دقیقاً ظهرِ روزِ شمسیِ خواسته‌شده باشد،
        /// تا تست‌ها به مرز نیمه‌شب و اختلاف ۳:۳۰ ساعته گیر نکنند.
        /// </summary>
        private static DateTime UtcForPersianDay(int year, int month, int day)
        {
            var iranNoon = Persian.ToDateTime(year, month, day, 12, 0, 0, 0);

            return DateTime.SpecifyKind(
                iranNoon - TimeSpan.FromMinutes(210), DateTimeKind.Utc);
        }

        [Theory]
        [InlineData(28, true)]   // اولین روز پنجره
        [InlineData(29, true)]
        [InlineData(30, true)]
        [InlineData(31, true)]   // شهریور ۳۱ روزه است
        [InlineData(1, true)]    // آخرین روز پنجره (ماه بعد)
        [InlineData(2, false)]   // روز پرداخت حقوق؛ دیگر دیر است
        [InlineData(15, false)]
        [InlineData(27, false)]  // یک روز مانده به باز شدن پنجره
        public void Employment_Change_Window_Wraps_Around_Month_End(int persianDay, bool expected)
        {
            var sut = CreateSut();

            // شهریور ۱۴۰۵ که ۳۱ روزه است
            var utc = UtcForPersianDay(1405, 6, persianDay);

            sut.IsWithinEmploymentChangeWindow(utc).Should().Be(expected);
        }

        [Fact]
        public void Employment_Window_Works_In_A_Thirty_Day_Month()
        {
            var sut = CreateSut();

            // آبان ۱۴۰۵ = ۳۰ روزه؛ روز ۳۱ وجود ندارد و نباید لازم باشد جایی
            // به تعداد روزهای ماه اشاره کنیم.
            sut.IsWithinEmploymentChangeWindow(UtcForPersianDay(1405, 8, 30)).Should().BeTrue();
            sut.IsWithinEmploymentChangeWindow(UtcForPersianDay(1405, 8, 27)).Should().BeFalse();
        }

        [Fact]
        public void Employment_Window_Works_In_Esfand()
        {
            var sut = CreateSut();

            var daysInEsfand = Persian.GetDaysInMonth(1405, 12);

            sut.IsWithinEmploymentChangeWindow(UtcForPersianDay(1405, 12, daysInEsfand))
                .Should().BeTrue();
        }

        [Theory]
        [InlineData(24, false)]
        [InlineData(25, true)]   // پنجره‌ی انتخاب روش پرداخت باز می‌شود
        [InlineData(31, true)]   // تا پایان ماه باز می‌ماند
        public void Payment_Method_Selection_Window_Opens_On_Day_25(int persianDay, bool expected)
        {
            var sut = CreateSut();

            var utc = UtcForPersianDay(1405, 6, persianDay);

            sut.IsWithinPaymentMethodSelectionWindow(utc).Should().Be(expected);
        }

        [Fact]
        public void Day_Boundary_Uses_Iran_Time_Not_Utc()
        {
            var sut = CreateSut();

            // ۲۱:۰۰ UTC روز ۲۷ام، در تهران ۰۰:۳۰ روز ۲۸ام است → باید داخل پنجره باشد.
            var iran28thJustAfterMidnight =
                Persian.ToDateTime(1405, 6, 28, 0, 30, 0, 0);

            var utc = DateTime.SpecifyKind(
                iran28thJustAfterMidnight - TimeSpan.FromMinutes(210), DateTimeKind.Utc);

            utc.Day.Should().NotBe(0); // فقط برای اطمینان از ساخت درست
            sut.GetPersianDayOfMonth(utc).Should().Be(28);
            sut.IsWithinEmploymentChangeWindow(utc).Should().BeTrue();
        }

        [Fact]
        public void Persian_Date_String_Is_Formatted_For_Display()
        {
            var sut = CreateSut();

            sut.ToPersianDateString(UtcForPersianDay(1405, 5, 19))
                .Should().Be("1405/05/19");
        }

        [Fact]
        public void Days_In_Persian_Month_Reflects_Real_Calendar()
        {
            var sut = CreateSut();

            sut.GetDaysInPersianMonth(UtcForPersianDay(1405, 1, 15)).Should().Be(31); // فروردین
            sut.GetDaysInPersianMonth(UtcForPersianDay(1405, 8, 15)).Should().Be(30); // آبان
        }
    }
}
