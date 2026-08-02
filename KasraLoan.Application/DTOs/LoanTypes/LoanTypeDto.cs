using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.LoanTypes
{
    public class LoanTypeDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>نام enum نوع وام (مثلاً MarriageLoan).</summary>
        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
