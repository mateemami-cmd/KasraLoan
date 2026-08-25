using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Employee
{
    /// <summary>
    /// فیلدهایی که ادمین از پنل ویرایش می‌تواند تغییر دهد.
    /// عمداً هیچ فیلدی برای امتیاز اینجا وجود ندارد؛ امتیاز فقط از اندپوینت
    /// مخصوص خودش (score) قابل تغییر است، نه از این مسیر.
    /// </summary>
    public class AdminUpdateEmployeeRequestDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        /// <summary>کد ملی (دقیقاً ۱۰ رقم).</summary>
        public string NationalId { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public DateTime HireDate { get; set; }

        public DateTime? MarriageDate { get; set; }

        /// <summary>"Employee" یا "Admin"</summary>
        public string Role { get; set; } = "Employee";

        public bool IsActive { get; set; }

        /// <summary>سمت شغلی. برای نقش Employee الزامی است.</summary>
        public int? JobPositionId { get; set; }

        /// <summary>
        /// حقوق ماهانه‌ی اختصاصی به تومان. اگر null باشد، حقوق پایه‌ی سمت شغلی
        /// استفاده می‌شود.
        /// </summary>
        public long? MonthlySalary { get; set; }
    }
}