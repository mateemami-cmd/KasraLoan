using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace KasraLoan.Application.Common.Exceptions
{
    /// <summary>
    /// برای خطاهای منطق تجاری قابل‌پیش‌بینی (نه باگ واقعی) استفاده می‌شود؛
    /// مثل نقض یک قانون وام، امتیاز ناکافی، یا تلاش برای تکرار یک عملیات غیرمجاز.
    /// به HTTP 400 Bad Request نگاشت می‌شود و پیامش مستقیم به کاربر نمایش داده می‌شود.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}