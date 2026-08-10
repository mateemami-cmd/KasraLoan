using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PersonnelNumber { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        /// <summary>
        /// شماره‌های تماس اضافه (اختیاری). کارمند می‌تواند هر چند شماره که بخواهد
        /// اضافه یا حذف کند. در PostgreSQL به‌صورت یک ستون text[] ذخیره می‌شود.
        /// </summary>
        public List<string>? AdditionalPhoneNumbers { get; set; }

        public string? Email { get; set; }

        /// <summary>مسیر نسبی عکس پروفایل، مثلاً /uploads/xxxx.jpg (اختیاری).</summary>
        public string? ProfilePictureUrl { get; set; }

        public DateTime HireDate { get; set; }

        public DateTime? MarriageDate { get; set; }

        public int? Year { get; set; }

        /// <summary>
        /// وضعیت <b>حساب کاربری</b>: آیا اجازه‌ی ورود به سیستم دارد یا نه.
        /// این با «مشغول به کار بودن» یکی نیست؛ آن را در
        /// <see cref="EmploymentStatus"/> ببینید.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>وضعیت اشتغال. فقط از اندپوینت اختصاصی خودش قابل تغییر است.</summary>
        public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

        /// <summary>تاریخ پایان همکاری، اگر وضعیت Terminated باشد.</summary>
        public DateTime? TerminationDate { get; set; }

        /// <summary>
        /// سمت شغلی. برای کارمند اجباری است و برای ادمین می‌تواند خالی بماند
        /// (اعتبارسنجی‌اش در CreateEmployeeValidator انجام می‌شود، نه اینجا).
        /// </summary>
        public int? JobPositionId { get; set; }

        public JobPosition? JobPosition { get; set; }

        /// <summary>
        /// حقوق ماهانه‌ی واقعی به تومان. اگر null باشد، حقوق پایه‌ی
        /// <see cref="JobPosition"/> استفاده می‌شود.
        /// همان الگوی <see cref="EmployeeScore.ManualOverrideScore"/>.
        /// </summary>
        public long? MonthlySalary { get; set; }

        public UserRole Role { get; set; } = UserRole.Employee;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}