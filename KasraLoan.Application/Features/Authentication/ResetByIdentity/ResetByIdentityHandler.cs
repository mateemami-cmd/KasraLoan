using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.ResetByIdentity
{
    public class ResetByIdentityHandler : IRequestHandler<ResetByIdentityCommand, ResetByIdentityResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ResetByIdentityHandler(IEmployeeRepository employeeRepository, IPasswordHasher passwordHasher)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<ResetByIdentityResponse> Handle(ResetByIdentityCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(request.Request.Username);

            // دوباره نام کاربری + کد ملی را بررسی می‌کنیم تا این مسیر بدونِ تأییدِ هویت
            // قابل سوءاستفاده نباشد.
            IdentityGuard.EnsureMatch(employee, request.Request.NationalId);

            employee!.PasswordHash = _passwordHasher.Hash(request.Request.NewPassword);
            employee.MustResetPassword = false;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new ResetByIdentityResponse { Message = "رمز عبور با موفقیت تغییر کرد. اکنون وارد شوید." };
        }
    }
}
