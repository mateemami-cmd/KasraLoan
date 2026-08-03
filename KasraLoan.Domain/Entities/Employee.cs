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

        public bool IsActive { get; set; }

        public UserRole Role { get; set; } = UserRole.Employee;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}