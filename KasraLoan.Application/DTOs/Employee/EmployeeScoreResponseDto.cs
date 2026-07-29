using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Employee
{
    public class EmployeeScoreResponseDto
    {
        public Guid EmployeeId { get; set; }

        public int MonthsEmployed { get; set; }

        public int AutomaticScore { get; set; }

        public int? ManualOverrideScore { get; set; }

        public int EffectiveScore { get; set; }

        public bool IsOverridden { get; set; }

        public int MinimumScoreRequiredForLoan { get; set; }

        /// <summary>
        /// آیا در حال حاضر یک مجوز یک‌بارمصرف درخواست وام فعال دارد؟
        /// </summary>
        public bool HasActiveLoanPermission { get; set; }
    }
}