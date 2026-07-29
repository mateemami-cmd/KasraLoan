using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Employee
{
    public class GrantLoanPermissionRequestDto
    {
        public List<string> Usernames { get; set; } = new();

        /// <summary>
        /// true = اعطای مجوز، false = لغو مجوزی که هنوز استفاده نشده.
        /// </summary>
        public bool Grant { get; set; } = true;
    }
}