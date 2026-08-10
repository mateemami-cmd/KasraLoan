using KasraLoan.Application.DTOs.Employee;
using MediatR;
using System;

namespace KasraLoan.Application.Features.Employee.Commands.SetEmploymentStatus
{
    public class SetEmploymentStatusCommand : IRequest<EmploymentStatusResponseDto>
    {
        public Guid EmployeeId { get; set; }

        public SetEmploymentStatusRequestDto Request { get; set; } = null!;
    }
}
