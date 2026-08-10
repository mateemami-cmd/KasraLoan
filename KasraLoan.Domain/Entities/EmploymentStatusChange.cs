using KasraLoan.Domain.Enums;
using System;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// تاریخچه‌ی تغییرات وضعیت اشتغال.
    ///
    /// صرفاً برای گزارش نیست: محاسبه‌ی درست امتیاز به آن نیاز دارد، چون ماه‌هایی
    /// که کارمند مشغول به کار نبوده نباید امتیاز بسازند. تا وقتی این جدول پر
    /// نشده باشد، امتیاز همچنان از HireDate تا امروز حساب می‌شود.
    /// </summary>
    public class EmploymentStatusChange
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public EmploymentStatus FromStatus { get; set; }

        public EmploymentStatus ToStatus { get; set; }

        /// <summary>توضیح ادمین: استعفا، پایان قرارداد، بازگشت به کار و ...</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>ادمینی که تغییر را ثبت کرده.</summary>
        public Guid ChangedByAdminId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
