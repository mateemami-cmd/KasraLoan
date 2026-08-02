using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.LoanTypes
{
    public class SetLoanTypeActiveStatusRequestDto
    {
        /// <summary>true = فعال، false = غیرفعال.</summary>
        public bool IsActive { get; set; }
    }
}
