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

        // تاریخ ایجاد رکورد
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    //public class LoanInstallment
    //{
    //    public int Id { get; set; }

    //    public int LoanRequestId { get; set; }

    //    public int InstallmentNumber { get; set; }

    //    public DateTime DueDate { get; set; }

    //    public decimal Amount { get; set; }

    //    public bool IsPaid { get; set; }

    //    public DateTime? PaidAt { get; set; }

    //    public int PenaltyAmount { get; set; }

    //    public LoanRequest LoanRequest { get; set; }
    //}
}