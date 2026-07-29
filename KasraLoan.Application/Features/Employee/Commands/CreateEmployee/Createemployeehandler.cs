using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.CreateEmployee
{
    public class CreateEmployeeHandler
        : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordGenerator _passwordGenerator;

        public CreateEmployeeHandler(
            IEmployeeRepository employeeRepository,
            IEmployeeScoreRepository employeeScoreRepository,
            IPasswordHasher passwordHasher,
            IPasswordGenerator passwordGenerator)
        {
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _passwordHasher = passwordHasher;
            _passwordGenerator = passwordGenerator;
        }

        public async Task<CreateEmployeeResponse> Handle(
            CreateEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request;

            if (await _employeeRepository.UsernameExistsAsync(dto.Username))
                throw new InvalidOperationException("این نام کاربری قبلاً استفاده شده است.");

            if (await _employeeRepository.PersonnelNumberExistsAsync(dto.PersonnelNumber))
                throw new InvalidOperationException("این شماره پرسنلی قبلاً ثبت شده است.");

            var role = UserRole.Employee;

            if (!string.IsNullOrWhiteSpace(dto.Role))
                Enum.TryParse(dto.Role, ignoreCase: true, out role);

            var temporaryPassword = _passwordGenerator.Generate();

            var employee = new Domain.Entities.Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PersonnelNumber = dto.PersonnelNumber,
                Username = dto.Username,
                PasswordHash = _passwordHasher.Hash(temporaryPassword),
                HireDate = dto.HireDate,
                MarriageDate = dto.MarriageDate,
                IsActive = true,
                Role = role
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            await _employeeScoreRepository.AddAsync(new EmployeeScore
            {
                EmployeeId = employee.Id,
                ManualOverrideScore = null,
                CreatedAt = DateTime.UtcNow
            });
            await _employeeScoreRepository.SaveChangesAsync();

            return new CreateEmployeeResponse
            {
                Id = employee.Id,
                Username = employee.Username,
                TemporaryPassword = temporaryPassword,
                Message = "کارمند با موفقیت ایجاد شد. این رمز موقت را فقط یک‌بار می‌بینید؛ آن را از طریق کانال امن به کارمند اطلاع دهید و به او بگویید در اولین ورود رمزش را تغییر دهد."
            };
        }
    }
}