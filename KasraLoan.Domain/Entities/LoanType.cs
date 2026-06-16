using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanType
    {
        public int Id { get; set; }

        public LoanTypeEnum Type { get; set; }

        public bool IsActive { get; set; }

        public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();

        public ICollection<LoanRule> LoanRules { get; set; } = new List<LoanRule>();
    }
}