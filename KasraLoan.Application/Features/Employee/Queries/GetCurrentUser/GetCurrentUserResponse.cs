using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Queries.GetCurrentUser
{
    public class GetCurrentUserResponse
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public List<string> AdditionalPhoneNumbers { get; set; } = new();

        public string? Email { get; set; }

        public int Score { get; set; }

        public string Role { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        public string? JobPositionTitle { get; set; }

        /// <summary>حقوق ماهانه‌ای که در محاسبات استفاده می‌شود.</summary>
        public long EffectiveMonthlySalary { get; set; }

        /// <summary>
        /// سقف قسط ماهانه. فرم درخواست وام از روی همین عدد، پیش از ارسال به سرور،
        /// به کارمند نشان می‌دهد چقدر می‌تواند بگیرد.
        /// </summary>
        public decimal MaxMonthlyInstallment { get; set; }

        /// <summary>وضعیت اشتغال؛ کارمند غیرفعال اجازه‌ی درخواست وام جدید ندارد.</summary>
        public string EmploymentStatus { get; set; } = string.Empty;
    }
}