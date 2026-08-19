using System;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Services.Auth
{
    /// <summary>
    /// نام کاربری کارمند را خودکار می‌سازد؛ دیگر ادمین آن را دستی وارد نمی‌کند.
    ///
    /// قالب: ۹ رقم به‌شکل «YYYY CC NNN»
    ///   YYYY = سال استخدام به هجری شمسی (از روی HireDate)
    ///   CC   = کد دو رقمی سمت شغلی
    ///   NNN  = شماره ترتیب؛ نفر چندمِ همان سمت که همان سال اضافه شده (از 001).
    /// شمارنده برای هر (سال + سمت) جداگانه از یک شروع می‌شود.
    /// </summary>
    public interface IUsernameGenerator
    {
        /// <summary>نام کاربری بعدیِ یک استخدام جدید در سمت داده‌شده را می‌سازد.</summary>
        Task<string> GenerateAsync(DateTime hireDate, JobPosition position);

        /// <summary>سال استخدام به هجری شمسی. مبنای بخش اول نام کاربری است.</summary>
        int GetHireYear(DateTime hireDate);

        /// <summary>سه بخش را به یک نام کاربری ۹ رقمی می‌چسباند.</summary>
        string Compose(int hireYear, string positionCode, int sequence);
    }
}
