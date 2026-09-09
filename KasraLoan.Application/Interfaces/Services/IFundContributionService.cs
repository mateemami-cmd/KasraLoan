using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// واریزِ ماهانه به صندوق: اولِ هر ماهِ شمسی، ۳٪ از حقوقِ هر کارمندِ فعال به
    /// موجودیِ صندوق اضافه می‌شود.
    /// </summary>
    public interface IFundContributionService
    {
        /// <param name="force">
        /// اگر false باشد و این ماهِ شمسی قبلاً واریز شده باشد، دوباره واریز نمی‌شود
        /// (استفاده‌ی زمان‌بند). اگر true باشد، بدونِ توجه به دوره واریز می‌کند (اجرای
        /// دستی برای تست/دمو).
        /// </param>
        Task<FundContributionResult> ApplyMonthlyContributionAsync(bool force = false);
    }

    public class FundContributionResult
    {
        /// <summary>آیا در این فراخوانی واریزی انجام شد.</summary>
        public bool Applied { get; set; }

        /// <summary>مبلغی که در این واریز به صندوق اضافه شد (تومان).</summary>
        public long AmountAdded { get; set; }

        /// <summary>تعداد کارمندانی که سهمشان محاسبه شد.</summary>
        public int EmployeeCount { get; set; }

        /// <summary>ماهِ شمسیِ این واریز، به شکل «yyyy/MM».</summary>
        public string Period { get; set; } = string.Empty;

        /// <summary>موجودیِ صندوق پس از این فراخوانی.</summary>
        public long NewBalance { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
