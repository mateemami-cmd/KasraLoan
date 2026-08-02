using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.LoanPermission
{
    public class CreateLoanPermissionRequestDto
    {
        public int LoanTypeId { get; set; }

        /// <summary>دلیل کارمند برای درخواست این وام.</summary>
        public string Reason { get; set; } = string.Empty;
    }
}
