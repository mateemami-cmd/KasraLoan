using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.LoanPermission
{
    public class RejectLoanPermissionRequestDto
    {
        /// <summary>پاسخ/دلیل ادمین برای رد کردن درخواست (اختیاری).</summary>
        public string? AdminResponse { get; set; }
    }
}
