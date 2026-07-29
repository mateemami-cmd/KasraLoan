using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.GrantLoanPermission
{
    public class GrantLoanPermissionCommand : IRequest<GrantLoanPermissionResponse>
    {
        public GrantLoanPermissionRequestDto Request { get; set; } = null!;
    }
}