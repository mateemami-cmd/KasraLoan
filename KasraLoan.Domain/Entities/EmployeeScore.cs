using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class EmployeeScore
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }

        /// <summary>
        /// مقدار override دستی که فقط ادمین می‌تواند تنظیم کند.
        /// اگر null باشد، امتیاز به‌صورت خودکار از روی سابقه‌ی کار (HireDate) محاسبه می‌شود.
        /// اگر مقدار داشته باشد، همین مقدار به‌جای محاسبه‌ی خودکار استفاده می‌شود.
        /// </summary>
        public int? ManualOverrideScore { get; set; }

        public DateTime? OverriddenAt { get; set; }

        /// <summary>
        /// مجوز استثنایی یک‌بارمصرف که ادمین می‌دهد: با وجود امتیاز کافی نداشتن،
        /// کارمند اجازه دارد یک درخواست وام ثبت کند. بعد از ثبت همان یک درخواست،
        /// این مقدار خودکار به false برمی‌گردد.
        /// </summary>
        public bool HasLoanPermissionOverride { get; set; }

        public DateTime? PermissionGrantedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Employee? Employee { get; set; }
    }
}