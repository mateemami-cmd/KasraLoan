using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateProfile
{
    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProfileHandler(IEmployeeRepository employeeRepository, IPasswordHasher passwordHasher, ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            var updatedFields = new List<string>();

            // نکته‌ی امنیتی مهم: عمداً هیچ‌کدام از این فیلدها آپدیت نمی‌شوند حتی اگر
            // در JSON درخواست فرستاده شده باشند: FirstName, LastName, Username,
            // PersonnelNumber, Role, IsActive. این‌ها فقط توسط ادمین قابل تغییرند.
            // این متد فقط دقیقاً همین سه فیلد را می‌خواند و چیز دیگری را لمس نمی‌کند.

            if (!string.IsNullOrWhiteSpace(request.Request.NewPassword))
            {
                employee.PasswordHash = _passwordHasher.Hash(request.Request.NewPassword);
                updatedFields.Add("رمز عبور");
            }

            if (!string.IsNullOrWhiteSpace(request.Request.PhoneNumber))
            {
                employee.PhoneNumber = request.Request.PhoneNumber;
                updatedFields.Add("شماره تماس");
            }

            if (!string.IsNullOrWhiteSpace(request.Request.SecondaryPhoneNumber))
            {
                employee.SecondaryPhoneNumber = request.Request.SecondaryPhoneNumber;
                updatedFields.Add("شماره تماس دوم");
            }

            if (!string.IsNullOrWhiteSpace(request.Request.Email))
            {
                employee.Email = request.Request.Email;
                updatedFields.Add("ایمیل");
            }

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new UpdateProfileResponse
            {
                Message = updatedFields.Count > 0
                    ? $"موارد زیر با موفقیت به‌روزرسانی شد: {string.Join("، ", updatedFields)}"
                    : "هیچ تغییری اعمال نشد."
            };
        }
    }
}