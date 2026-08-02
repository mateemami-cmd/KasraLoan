using KasraLoan.Application.DTOs.LoanPermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Queries.GetAllLoanPermissionRequests
{
    public class GetAllLoanPermissionRequestsResponse
    {
        public List<LoanPermissionRequestListItemDto> Items { get; set; } = new();

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
