using System;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// صندوقِ وام: منبعِ پولی که وام‌های تأییدشده از آن پرداخت می‌شوند. یک ردیفِ
    /// واحد (Id = 1) در دیتابیس دارد. با تأییدِ هر وام، مبلغِ تأییدشده از موجودی کم
    /// می‌شود.
    /// </summary>
    public class LoanFund
    {
        public int Id { get; set; }

        /// <summary>موجودیِ فعلیِ صندوق به تومان.</summary>
        public long Balance { get; set; }

        /// <summary>
        /// آخرین ماهِ شمسی‌ای که واریزِ ماهانه (۳٪ حقوق) انجام شد، به شکل «yyyy/MM».
        /// برای جلوگیری از واریزِ تکراری در یک ماه.
        /// </summary>
        public string? LastContributionPeriod { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
