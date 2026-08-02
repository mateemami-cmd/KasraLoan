using KasraLoan.Application.DTOs.LoanPermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Queries.GetMyLoanPermissionRequests
{
    public class GetMyLoanPermissionRequestsResponse
    {
        public List<LoanPermissionRequestListItemDto> Items { get; set; } = new();
    }
}
