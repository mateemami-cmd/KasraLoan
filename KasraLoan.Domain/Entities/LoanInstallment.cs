using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanInstallment
    {
        public Guid Id { get; set; }

        // ارتباط با درخواست وام
        public Guid LoanRequestId { get; set; }
        public LoanRequest LoanRequest { get; set; }

        // شماره قسط (1، 2، 3، ...)
        public int InstallmentNumber { get; set; }

        // مبلغ قسط
        public decimal Amount { get; set; }

        // تاریخ سررسید
        public DateTime DueDate { get; set; }

        // وضعیت پرداخت
        public bool IsPaid { get; set; }

        // تاریخ پرداخت واقعی (اگر پرداخت شده باشد)
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// روشی که این قسط با آن تسویه شد (یا قرار است بشود).
        /// خلاصه‌ی رکورد تأییدشده در <see cref="Payments"/> است تا برای نمایش
        /// لازم نباشد هر بار تاریخچه‌ی پرداخت‌ها خوانده شود.
        /// </summary>
        public PaymentMethod? PaidMethod { get; set; }

        /// <summary>
        /// تاریخچه‌ی تلاش‌های پرداخت این قسط؛ شامل چک‌های ردشده و پرداخت‌های ناموفق.
        /// </summary>
        public ICollection<InstallmentPayment> Payments { get; set; } = new List<InstallmentPayment>();

        // تاریخ ایجاد رکورد
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}