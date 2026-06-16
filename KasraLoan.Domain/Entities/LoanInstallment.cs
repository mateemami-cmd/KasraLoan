using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanInstallment
    {
        public int Id { get; set; }

        public int LoanRequestId { get; set; }

        public int InstallmentNumber { get; set; }

        public DateTime DueDate { get; set; }

        public int Amount { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidAt { get; set; }

        public int PenaltyAmount { get; set; }

        public LoanRequest LoanRequest { get; set; }
    }
}