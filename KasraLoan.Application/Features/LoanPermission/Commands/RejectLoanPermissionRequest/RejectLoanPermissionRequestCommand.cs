using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.RejectLoanPermissionRequest
{
    public class RejectLoanPermissionRequestCommand : IRequest<RejectLoanPermissionRequestResponse>
    {
        public Guid PermissionRequestId { get; set; }

        public string? AdminResponse { get; set; }
    }
}
