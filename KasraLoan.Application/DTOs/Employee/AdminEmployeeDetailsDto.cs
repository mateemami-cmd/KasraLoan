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

        public bool IsActive { get; set; }
    }
}