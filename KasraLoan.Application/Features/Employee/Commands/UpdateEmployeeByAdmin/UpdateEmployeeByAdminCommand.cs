using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateEmployeeByAdmin
{
    public class UpdateEmployeeByAdminCommand : IRequest<AdminEmployeeDetailsDto>
    {
        public Guid EmployeeId { get; set; }

        public AdminUpdateEmployeeRequestDto Request { get; set; } = null!;
    }
}