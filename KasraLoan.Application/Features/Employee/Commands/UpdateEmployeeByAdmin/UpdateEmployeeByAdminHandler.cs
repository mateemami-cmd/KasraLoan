using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateEmployeeByAdmin
{
    public class UpdateEmployeeByAdminHandler
        : IRequestHandler<UpdateEmployeeByAdminCommand, AdminEmployeeDetailsDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IEmployeeSalaryService _employeeSalaryService;

        public UpdateEmployeeByAdminHandler(
            IEmployeeRepository employeeRepository,
            IJobPositionRepository jobPositionRepository,
            IEmployeeSalaryService employeeSalaryService)
        {
            _employeeRepository = employeeRepository;
            _jobPositionRepository = jobPositionRepository;
            _employeeSalaryService = employeeSalaryService;
        }

        public async Task<AdminEmployeeDetailsDto> Handle(
            UpdateEmployeeByAdminCommand request,
            CancellationToken cancellationToken)
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
                    throw new BusinessRuleException("این نام کاربری قبلاً برای کارمند دیگری استفاده شده است.");
            }

            // همین‌طور برای شماره پرسنلی.
            if (!string.Equals(employee.PersonnelNumber, dto.PersonnelNumber, StringComparison.Ordinal))
            {
                var ownerOfPersonnelNumber = await _employeeRepository.GetByPersonnelNumberAsync(dto.PersonnelNumber);

                if (ownerOfPersonnelNumber != null && ownerOfPersonnelNumber.Id != employee.Id)
                    throw new BusinessRuleException("این شماره پرسنلی قبلاً برای کارمند دیگری ثبت شده است.");
            }

            if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
                role = employee.Role;

            Domain.Entities.JobPosition? newJobPosition = null;

            if (dto.JobPositionId.HasValue)
            {
                newJobPosition = await _jobPositionRepository.GetByIdAsync(dto.JobPositionId.Value);

                if (newJobPosition == null)
                    throw new BusinessRuleException("سمت شغلی انتخاب‌شده یافت نشد.");

                // سمت غیرفعال فقط وقتی مجاز است که کارمند از قبل همان سمت را داشته باشد
                // (تا ویرایش سایر فیلدهایش قفل نشود).
                if (!newJobPosition.IsActive && employee.JobPositionId != newJobPosition.Id)
                    throw new BusinessRuleException("سمت شغلی انتخاب‌شده غیرفعال است.");
            }
            else if (role != UserRole.Admin)
            {
                throw new BusinessRuleException("انتخاب سمت شغلی برای کارمند الزامی است.");
            }

            if (dto.MonthlySalary.HasValue && dto.MonthlySalary.Value <= 0)
                throw new BusinessRuleException("حقوق ماهانه باید بزرگ‌تر از صفر باشد.");

            // نکته‌ی امنیتی مهم: این هندلر عمداً به EmployeeScore هیچ دسترسی و ارجاعی ندارد.
            // امتیاز فقط از طریق SetEmployeeScoreOverrideHandler قابل تغییر است، نه از اینجا.
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Username = dto.Username;
            employee.PersonnelNumber = dto.PersonnelNumber;
            employee.NationalId = dto.NationalId?.Trim();
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Email = dto.Email;
            employee.HireDate = dto.HireDate;
            employee.MarriageDate = dto.MarriageDate;
            employee.Role = role;
            employee.IsActive = dto.IsActive;
            employee.JobPositionId = dto.JobPositionId;
            employee.MonthlySalary = dto.MonthlySalary;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            // navigation property را هم‌راستا می‌کنیم تا محاسبه‌ی حقوق مؤثر پایین
            // بر اساس سمت جدید انجام شود، نه سمت قبلیِ بارگذاری‌شده.
            employee.JobPosition = newJobPosition;

            return new AdminEmployeeDetailsDto
            {
                JobPositionId = employee.JobPositionId,
                JobPositionTitle = newJobPosition?.Title,
                MonthlySalary = employee.MonthlySalary,
                EffectiveMonthlySalary = _employeeSalaryService.GetEffectiveMonthlySalary(employee),
                MaxMonthlyInstallment = _employeeSalaryService.GetMaxMonthlyInstallment(employee),
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Username = employee.Username,
                PersonnelNumber = employee.PersonnelNumber ?? "",
                NationalId = employee.NationalId,
                PhoneNumber = employee.PhoneNumber,
                Email = employee.Email,
                HireDate = employee.HireDate,
                MarriageDate = employee.MarriageDate,
                Role = employee.Role.ToString(),
                IsActive = employee.IsActive,
                // EmploymentStatus عمداً از این هندلر تغییر نمی‌کند؛ فقط برگردانده می‌شود.
                EmploymentStatus = employee.EmploymentStatus.ToString(),
                TerminationDate = employee.TerminationDate
            };
        }
    }
}