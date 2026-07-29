using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetEmployeeScoreOverride
{
    public class SetEmployeeScoreOverrideCommand : IRequest<EmployeeScoreResponseDto>
    {
        public Guid EmployeeId { get; set; }

        public SetEmployeeScoreOverrideRequestDto Request { get; set; } = null!;
    }
}