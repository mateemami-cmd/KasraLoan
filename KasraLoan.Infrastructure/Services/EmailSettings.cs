namespace KasraLoan.Infrastructure.Services
{
    /// <summary>
    /// تنظیماتِ SMTP برای ارسال ایمیل. مقادیرِ حساس (User/Password) در User Secrets
    /// می‌آیند، نه در appsettings. برای Gmail: Host=smtp.gmail.com, Port=587,
    /// User=آدرسِ جیمیلِ فرستنده, Password=App Password ۱۶ رقمی.
    /// </summary>
    public class EmailSettings
    {
        public bool Enabled { get; set; }

        public string Host { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        /// <summary>نامی که به‌عنوان فرستنده نمایش داده می‌شود.</summary>
        public string FromName { get; set; } = "صندوق همیار کسرا";
    }
}
