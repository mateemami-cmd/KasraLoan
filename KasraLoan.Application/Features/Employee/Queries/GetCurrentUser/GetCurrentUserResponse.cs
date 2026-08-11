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

        /// <summary>حداقل امتیاز لازم برای وام، تا فرانت آن را هاردکد نکند.</summary>
        public int MinimumScoreRequiredForLoan { get; set; }

        /// <summary>آیا مجوز استثنایی یک‌بارمصرف برای کارمند فعال است.</summary>
        public bool HasLoanPermission { get; set; }

        /// <summary>
        /// آیا کارمند در این لحظه می‌تواند درخواست وام بدهد.
        ///
        /// فرانت باید همین را ببیند و نه فقط امتیاز خام: کارمندی که امتیازش کم
        /// است ولی ادمین به او مجوز استثنایی داده، مجاز است. قبلاً داشبورد فقط
        /// امتیاز را چک می‌کرد و دکمه را غیرفعال می‌گذاشت، یعنی مجوزِ صادرشده
        /// عملاً از رابط کاربری قابل استفاده نبود.
        /// </summary>
        public bool CanRequestLoan { get; set; }
    }
}