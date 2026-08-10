using System;
using System.Collections.Generic;

namespace KasraLoan.Application.Features.Loan.Queries.GetMyLoans.GetAllLoans
{
    public class LoanListItemDto
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeUsername { get; set; } = string.Empty;

        public int LoanTypeId { get; set; }

        public string LoanTypeName { get; set; } = string.Empty;

        public long RequestedAmount { get; set; }

        public long ApprovedAmount { get; set; }

        public int InstallmentCount { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>اصل + کارمزد. تا قبل از تأیید صفر است.</summary>
        public long TotalPayableAmount { get; set; }

        public decimal MonthlyPaymentAmount { get; set; }

        public decimal AnnualFeePercent { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class GetAllLoansResponse
    {
        public List<LoanListItemDto> Items { get; set; } = new();

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}