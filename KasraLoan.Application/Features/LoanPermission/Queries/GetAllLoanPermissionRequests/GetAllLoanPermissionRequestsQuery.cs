using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Queries.GetAllLoanPermissionRequests
{
    public class GetAllLoanPermissionRequestsQuery : IRequest<GetAllLoanPermissionRequestsResponse>
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public LoanPermissionRequestStatus? Status { get; set; }
    }
}
