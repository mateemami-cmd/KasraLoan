using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Queries.GetEmployeeById
{
    public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, AdminEmployeeDetailsDto>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetEmployeeByIdHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<AdminEmployeeDetailsDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var e = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (e == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            return new AdminEmployeeDetailsDto
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
            };
        }
    }
}