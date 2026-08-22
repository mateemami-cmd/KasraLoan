using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services.Auth
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }

        string? FirstName { get; }

        string? PersonnelNumber { get; }

        string? Role { get; }

        /// <summary>ادمین ارشد است؟ (از claim توکن).</summary>
        bool IsSeniorAdmin { get; }

        /// <summary>اگر ادمین وام باشد، شناسه‌ی وامی که مدیریت می‌کند؛ وگرنه null.</summary>
        int? ManagedLoanTypeId { get; }

        /// <summary>شناسه‌ی نشستِ جاری (Id رفرش‌توکن) از claim توکن.</summary>
        int? SessionId { get; }

        /// <summary>
        /// آیا کاربر جاری اجازه‌ی مدیریت این نوع وام را دارد؟ ادمین ارشد همیشه بله؛
        /// ادمین وام فقط برای وامی که به او سپرده شده.
        /// </summary>
        bool CanManageLoanType(int loanTypeId);

        bool IsAuthenticated { get; }
    }
}