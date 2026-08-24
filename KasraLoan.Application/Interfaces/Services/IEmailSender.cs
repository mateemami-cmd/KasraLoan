using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// ارسال ایمیل (مثلاً رمزِ موقتِ فراموشیِ رمز عبور). پیاده‌سازیِ واقعی از SMTP
    /// استفاده می‌کند؛ اگر SMTP تنظیم نشده باشد، به‌جای ارسال، محتوا را لاگ می‌کند
    /// تا در حالتِ توسعه بدون ایمیلِ واقعی هم بشود جریان را تست کرد.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>ایمیل را می‌فرستد. اگر واقعاً ارسال شد true، اگر فقط لاگ شد false.</summary>
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody);

        /// <summary>آیا SMTP تنظیم و فعال است (یعنی ایمیلِ واقعی می‌رود).</summary>
        bool IsConfigured { get; }
    }
}
