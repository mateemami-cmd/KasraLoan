using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// درخواست مجوز وام از سمت کارمند: وقتی امتیاز کارمند برای گرفتن وام کافی نیست،
    /// می‌تواند نوع وام موردنظر و دلیلش را بنویسد و برای ادمین بفرستد. ادمین در صورت
    /// موافقت، یک مجوز یک‌بارمصرف روی حساب کارمند فعال می‌کند.
    /// </summary>
    public class LoanPermissionRequest
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        /// <summary>دلیل کارمند برای درخواست این وام.</summary>
        public string Reason { get; set; } = string.Empty;

        public LoanPermissionRequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        /// <summary>پاسخ ادمین (مثلاً دلیل رد شدن). اختیاری.</summary>
        public string? AdminResponse { get; set; }

        public Employee? Employee { get; set; }

        public LoanType? LoanType { get; set; }
    }
}
