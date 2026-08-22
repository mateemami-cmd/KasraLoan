using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ChangePassword
{
    public class ChangePasswordHandler
        : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;

        public ChangePasswordHandler(
            IEmployeeRepository employeeRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public async Task<ChangePasswordResponse> Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            // رمز فعلی باید درست باشد؛ وگرنه اجازه‌ی تغییر نمی‌دهیم.
            if (!_passwordHasher.Verify(request.Request.CurrentPassword, employee.PasswordHash))
                throw new BusinessRuleException("رمز عبور فعلی اشتباه است.");

            employee.PasswordHash = _passwordHasher.Hash(request.Request.NewPassword);

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new ChangePasswordResponse { Message = "رمز عبور با موفقیت تغییر کرد." };
        }
    }
}
