using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordHandler
        : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;

        public ResetPasswordHandler(
            IEmployeeRepository employeeRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public async Task<ResetPasswordResponse> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            // این مسیر بدونِ رمزِ فعلی رمز را عوض می‌کند، پس فقط وقتی مجاز است که
            // رمزِ کاربر واقعاً موقت باشد (از فراموشیِ رمز). وگرنه راهِ دور زدنِ
            // «رمز فعلی» می‌شد.
            if (!employee.MustResetPassword)
                throw new BusinessRuleException("این عملیات فقط زمانی مجاز است که رمزِ شما موقت باشد.");

            employee.PasswordHash = _passwordHasher.Hash(request.Request.NewPassword);
            employee.MustResetPassword = false;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new ResetPasswordResponse { Message = "رمز عبور با موفقیت تنظیم شد." };
        }
    }
}
