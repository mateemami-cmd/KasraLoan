using KasraLoan.Application.DTOs.LoanPermission;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.CreateLoanPermissionRequest
{
    public class CreateLoanPermissionRequestCommand : IRequest<CreateLoanPermissionRequestResponse>
    {
        public CreateLoanPermissionRequestDto Request { get; set; } = null!;
    }
}
