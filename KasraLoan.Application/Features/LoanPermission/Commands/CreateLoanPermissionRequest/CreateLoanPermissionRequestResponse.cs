using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.CreateLoanPermissionRequest
{
    public class CreateLoanPermissionRequestResponse
    {
        public Guid RequestId { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
