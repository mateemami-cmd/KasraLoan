using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class LoanRequest
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public int LoanTypeId { get; set; }

        public long RequestedAmount { get; set; }

        public long ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public decimal MonthlyFeePercent { get; set; }

        public LoanStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public long TotalPayableAmount { get; set; }

        public decimal MonthlyPaymentAmount { get; set; }

        public string? RejectReason { get; set; }

        public Employee Employee { get; set; }

        public LoanType LoanType { get; set; }

        public ICollection<LoanInstallment> LoanInstallments { get; set; } = new List<LoanInstallment>();

        public ICollection<LoanDocument> LoanDocuments { get; set; } = new List<LoanDocument>();
    }
}