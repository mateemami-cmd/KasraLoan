using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Enums;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateEmployeeByAdmin
{
    public class UpdateEmployeeByAdminHandler : IRequestHandler<UpdateEmployeeByAdminCommand, AdminEmployeeDetailsDto>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public UpdateEmployeeByAdminHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<AdminEmployeeDetailsDto> Handle(UpdateEmployeeByAdminCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            var dto = request.Request;

            // اگر یوزرنیم تغییر کرده، مطمئن شویم متعلق به کارمند دیگری نیست.
            if (!string.Equals(employee.Username, dto.Username, StringComparison.Ordinal))
            {
                var ownerOfUsername = await _employeeRepository.GetByUsernameAsync(dto.Username);

                if (ownerOfUsername != null && ownerOfUsername.Id != employee.Id)
                    throw new InvalidOperationException("این نام کاربری قبلاً برای کارمند دیگری استفاده شده است.");
            }

            // همین‌طور برای شماره پرسنلی.
            if (!string.Equals(employee.PersonnelNumber, dto.PersonnelNumber, StringComparison.Ordinal))
            {
                var ownerOfPersonnelNumber = await _employeeRepository.GetByPersonnelNumberAsync(dto.PersonnelNumber);

                if (ownerOfPersonnelNumber != null && ownerOfPersonnelNumber.Id != employee.Id)
                    throw new InvalidOperationException("این شماره پرسنلی قبلاً برای کارمند دیگری ثبت شده است.");
            }

            if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
                role = employee.Role;

            // نکته‌ی امنیتی مهم: این هندلر عمداً به EmployeeScore هیچ دسترسی و ارجاعی ندارد.
            // امتیاز فقط از طریق SetEmployeeScoreOverrideHandler قابل تغییر است، نه از اینجا.
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Username = dto.Username;
            employee.PersonnelNumber = dto.PersonnelNumber;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Email = dto.Email;
            employee.HireDate = dto.HireDate;
            employee.MarriageDate = dto.MarriageDate;
            employee.Role = role;
            employee.IsActive = dto.IsActive;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new AdminEmployeeDetailsDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Username = employee.Username,
                PersonnelNumber = employee.PersonnelNumber ?? "",
                PhoneNumber = employee.PhoneNumber,
                Email = employee.Email,
                HireDate = employee.HireDate,
                MarriageDate = employee.MarriageDate,
                Role = employee.Role.ToString(),
                IsActive = employee.IsActive
            };
        }
    }
}