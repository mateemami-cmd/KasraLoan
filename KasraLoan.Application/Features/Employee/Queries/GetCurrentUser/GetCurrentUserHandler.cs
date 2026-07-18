using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Queries.GetCurrentUser
{
    public class GetCurrentUserHandler
        : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IEmployeeRepository _employeeRepository;

        public GetCurrentUserHandler(
            ICurrentUserService currentUser,
            IEmployeeRepository employeeRepository)
        {
            _currentUser = currentUser;
            _employeeRepository = employeeRepository;
        }

        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request,CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var employee = await _employeeRepository.GetByIdAsync(_currentUser.UserId);

            if (employee == null)
                throw new Exception("Employee not found.");

            return new GetCurrentUserResponse
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                PersonnelNumber = employee.PersonnelNumber ?? "",
                Role = employee.Role.ToString(),

                // فعلاً تا سرویس امتیاز را وصل نکردیم
                Score = 0
            };
        }
    }
}