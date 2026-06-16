using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        public int RequestedAmount { get; set; }

        public int ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public int TotalPayableAmount { get; set; }

        public decimal MonthlyPaymentAmount { get; set; }

        public string? RejectReason { get; set; }

        public Employee Employee { get; set; }

        public LoanType LoanType { get; set; }

        public ICollection<LoanInstallment> LoanInstallments { get; set; }
            = new List<LoanInstallment>();
    }
}