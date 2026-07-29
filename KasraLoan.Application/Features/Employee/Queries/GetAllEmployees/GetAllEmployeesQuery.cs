using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Queries.GetAllEmployees
{
    public class GetAllEmployeesQuery : IRequest<List<AdminEmployeeDetailsDto>>
    {
    }
}