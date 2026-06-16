using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanRule
    {
        public int Id { get; set; }

        public int LoanTypeId { get; set; }

        public int MinScore { get; set; }

        public int MaxAmount { get; set; }

        public int InterestRate { get; set; }

        public int MinWorkMonths { get; set; }

        public bool IsActive { get; set; }

        public LoanType LoanType { get; set; }
    }
}