using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetAdminDashboard
{
    public class GetAdminDashboardResponse
    {
        public int TotalLoans { get; set; }

        public int PendingLoans { get; set; }

        public int ApprovedLoans { get; set; }

        public int RejectedLoans { get; set; }

        public decimal TotalRequestedAmount { get; set; }

        public decimal TotalApprovedAmount { get; set; }
    }
}