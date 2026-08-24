using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Employee
{
    /// <summary>
    /// جزئیات کامل کارمند برای پنل ادمین. عمداً هیچ فیلد مربوط به امتیاز اینجا نیست؛
    /// امتیاز فقط از اندپوینت جدای GetEmployeeScore/SetEmployeeScoreOverride مدیریت می‌شود.
    /// </summary>
    public class AdminEmployeeDetailsDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public DateTime HireDate { get; set; }

        public DateTime? MarriageDate { get; set; }

        public string Role { get; set; } = string.Empty;

        /// <summary>برای ادمین‌ها: ارشد است یا ادمین وام.</summary>
        public bool IsSeniorAdmin { get; set; }

        /// <summary>برای «ادمین وام»: شناسه و نام وامی که مدیریت می‌کند.</summary>
        public int? ManagedLoanTypeId { get; set; }

        public string? ManagedLoanTypeName { get; set; }

        /// <summary>وضعیت حساب کاربری (اجازه‌ی ورود به سیستم).</summary>
        public bool IsActive { get; set; }

        /// <summary>حذفِ نرم شده؟ (ردیف و سوابق می‌مانند، فقط از فهرست‌های عادی کنار می‌رود.)</summary>
        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        /// <summary>وضعیت اشتغال: "Active" یا "Terminated". با IsActive یکی نیست.</summary>
        public string EmploymentStatus { get; set; } = string.Empty;

        public DateTime? TerminationDate { get; set; }

        public int? JobPositionId { get; set; }

        public string? JobPositionTitle { get; set; }

        /// <summary>حقوق اختصاصی، اگر ثبت شده باشد.</summary>
        public long? MonthlySalary { get; set; }

        /// <summary>حقوقی که واقعاً در محاسبات استفاده می‌شود (اختصاصی یا پایه‌ی سمت).</summary>
        public long EffectiveMonthlySalary { get; set; }

        /// <summary>سقف قسط ماهانه بر اساس حقوق مؤثر.</summary>
        public decimal MaxMonthlyInstallment { get; set; }
    }
}