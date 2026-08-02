using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.ApproveLoanPermissionRequest
{
    public class ApproveLoanPermissionRequestCommand : IRequest<ApproveLoanPermissionRequestResponse>
    {
        public Guid PermissionRequestId { get; set; }
    }
}
