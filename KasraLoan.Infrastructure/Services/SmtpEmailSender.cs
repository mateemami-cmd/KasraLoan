using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KasraLoan.Infrastructure.Services
{
    /// <summary>
    /// ارسال ایمیل با SMTP (سازگار با Gmail). اگر تنظیمات ناقص/غیرفعال باشد، به‌جای
    /// ارسال، محتوا را لاگ می‌کند تا در حالتِ توسعه بدونِ ایمیلِ واقعی هم جریانِ
    /// «فراموشی رمز» قابل تست باشد.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool IsConfigured =>
            _settings.Enabled
            && !string.IsNullOrWhiteSpace(_settings.Host)
            && !string.IsNullOrWhiteSpace(_settings.User)
            && !string.IsNullOrWhiteSpace(_settings.Password);

        public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
        {
            if (!IsConfigured)
            {
                // حالتِ توسعه: SMTP تنظیم نشده، پس فقط لاگ می‌کنیم تا ایمیل گم نشود.
                _logger.LogWarning(
                    "SMTP not configured — email NOT sent. To: {To} | Subject: {Subject}\n{Body}",
                    toEmail, subject, htmlBody);
                return false;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.User, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.User, _settings.Password),
            };

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Password-reset email sent to {To}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", toEmail);
                throw;
            }
        }
    }
}
