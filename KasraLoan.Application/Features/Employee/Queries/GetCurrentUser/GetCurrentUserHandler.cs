using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
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
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly IEmployeeScoreService _employeeScoreService;
        private readonly IEmployeeSalaryService _employeeSalaryService;

        public GetCurrentUserHandler(
            ICurrentUserService currentUser,
            IEmployeeRepository employeeRepository,
            IEmployeeScoreRepository employeeScoreRepository,
            IEmployeeScoreService employeeScoreService,
            IEmployeeSalaryService employeeSalaryService)
        {
            _currentUser = currentUser;
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _employeeScoreService = employeeScoreService;
            _employeeSalaryService = employeeSalaryService;
        }

        public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var employee = await _employeeRepository.GetByIdAsync(_currentUser.UserId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found.");

            var scoreRecord = await _employeeScoreRepository.GetByEmployeeIdAsync(employee.Id);

            var effectiveScore = _employeeScoreService.GetEffectiveScore(employee, scoreRecord);

            return new GetCurrentUserResponse
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Username = employee.Username,
                PersonnelNumber = employee.PersonnelNumber ?? "",
                PhoneNumber = employee.PhoneNumber,
                AdditionalPhoneNumbers = employee.AdditionalPhoneNumbers ?? new List<string>(),
                Email = employee.Email,
                Role = employee.Role.ToString(),
                Score = effectiveScore,
                ProfilePictureUrl = employee.ProfilePictureUrl,
                JobPositionTitle = employee.JobPosition?.Title,
                EffectiveMonthlySalary = _employeeSalaryService.GetEffectiveMonthlySalary(employee),
                MaxMonthlyInstallment = _employeeSalaryService.GetMaxMonthlyInstallment(employee),
                EmploymentStatus = employee.EmploymentStatus.ToString()
            };
        }
    }
}