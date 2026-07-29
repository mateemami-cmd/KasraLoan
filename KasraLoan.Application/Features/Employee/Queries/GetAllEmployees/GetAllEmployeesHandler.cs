using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Queries.GetAllEmployees
{
    public class GetAllEmployeesHandler
        : IRequestHandler<GetAllEmployeesQuery, List<AdminEmployeeDetailsDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetAllEmployeesHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<List<AdminEmployeeDetailsDto>> Handle(
            GetAllEmployeesQuery request,
            CancellationToken cancellationToken)
        {
            var employees = await _employeeRepository.GetAllAsync();

            return employees.Select(e => new AdminEmployeeDetailsDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Username = e.Username,
                PersonnelNumber = e.PersonnelNumber ?? "",
                PhoneNumber = e.PhoneNumber,
                Email = e.Email,
                HireDate = e.HireDate,
                MarriageDate = e.MarriageDate,
                Role = e.Role.ToString(),
                IsActive = e.IsActive
            })
                .ToList();
        }
    }
}