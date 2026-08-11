using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanRequest
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        public long RequestedAmount { get; set; }

        public long ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        /// <summary>
        /// کارمزد صندوق به‌صورت درصد <b>سالانه</b> روی اصل مبلغ تأییدشده.
        /// فرمول: کارمزد کل = مبلغ تأییدشده × (درصد ÷ ۱۰۰) × (تعداد اقساط ÷ ۱۲)
        /// </summary>
        public decimal AnnualFeePercent { get; set; }

        public LoanStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public long TotalPayableAmount { get; set; }

        public decimal MonthlyPaymentAmount { get; set; }

        public string? RejectReason { get; set; }

        /// <summary>
        /// آیا این وام برای تأیید مدرک لازم دارد.
        /// در لحظه‌ی ثبت درخواست از قانون نوع وام گرفته و اینجا ثبت می‌شود، تا
        /// اگر بعداً قانون عوض شد، وام‌های در جریان با شرایط روز خودشان بمانند.
        /// </summary>
        public bool RequiresDocument { get; set; }

        /// <summary>توضیح مدرک لازم؛ به کارمند و ادمین نمایش داده می‌شود.</summary>
        public string? RequiredDocumentDescription { get; set; }

        // ───── تسویه‌ی زودهنگام ─────
        // وقتی کل مانده‌ی وام یکجا مطالبه می‌شود و دیگر منتظر اقساط ماهانه نمی‌مانیم.
        // فعلاً تنها محرکش پایان همکاری است (حقوقی نمانده که ازش کسر شود)، ولی
        // مدل عمداً عمومی است تا بعداً «می‌خواهم زودتر تسویه کنم» هم روی همین بنشیند.

        /// <summary>مهلت پرداخت کل مانده. اگر null باشد، وام روال عادی اقساط را دارد.</summary>
        public DateTime? SettlementDueDate { get; set; }

        /// <summary>لحظه‌ی مطالبه‌ی تسویه.</summary>
        public DateTime? SettlementDemandedAt { get; set; }

        /// <summary>مانده‌ی وام در لحظه‌ی مطالبه (تومان). برای رکورد نگه داشته می‌شود.</summary>
        public long SettlementAmount { get; set; }

        /// <summary>چرا تسویه مطالبه شد؛ مثلاً «پایان همکاری».</summary>
        public string? SettlementReason { get; set; }

        /// <summary>آیا کل مانده یکجا مطالبه شده است.</summary>
        public bool IsSettlementDemanded => SettlementDemandedAt.HasValue;

        public Employee Employee { get; set; }

        public LoanType LoanType { get; set; }

        public ICollection<LoanInstallment> LoanInstallments { get; set; } = new List<LoanInstallment>();

        public ICollection<LoanDocument> LoanDocuments { get; set; } = new List<LoanDocument>();
    }
}