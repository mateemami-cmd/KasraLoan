using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Common.Results
{
    public class LoanRequestResult
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public int? LoanRequestId { get; set; }
    }
}