using KasraLoan.Application.Common;
using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginHandler(IEmployeeRepository employeeRepository, IPasswordHasher passwordHasher, IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        // هر کاربر حداکثر روی این تعداد دستگاه هم‌زمان می‌تواند وارد باشد.
        private const int MaxActiveSessions = 3;

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(request.LoginRequest.Username);

            const string invalidCredentialsMessage = "نام کاربری یا رمز عبور اشتباه است.";

            if (employee == null || !employee.IsActive)
                throw new UnauthorizedAccessException(invalidCredentialsMessage);

            var isPasswordValid = _passwordHasher.Verify(request.LoginRequest.Password, employee.PasswordHash);

            if (!isPasswordValid)
                throw new UnauthorizedAccessException(invalidCredentialsMessage);

            var now = DateTime.UtcNow;

            // اگر کاربر نشستی را برای قطع انتخاب کرده (چون به سقف رسیده بود)، همان را
            // باطل می‌کنیم تا جا باز شود. فقط نشستِ خودِ همین کاربر قابل قطع است.
            if (request.LoginRequest.TerminateSessionId.HasValue)
            {
                var toRevoke = await _refreshTokenRepository.GetByIdAsync(
                    request.LoginRequest.TerminateSessionId.Value);

                if (toRevoke != null && toRevoke.EmployeeId == employee.Id && !toRevoke.Revoked)
                {
                    toRevoke.Revoked = true;
                    await _refreshTokenRepository.UpdateAsync(toRevoke);
                }
            }

            // سقفِ نشست‌های هم‌زمان. اگر پر بود، به‌جای ورود، لیستِ نشست‌های فعال را
            // برمی‌گردانیم تا کاربر یکی را برای قطع انتخاب کند.
            var activeSessions = await _refreshTokenRepository.GetActiveByEmployeeAsync(employee.Id);

            if (activeSessions.Count >= MaxActiveSessions)
            {
                return new LoginResponseDto
                {
                    RequiresSessionChoice = true,
                    Sessions = activeSessions.Select(ToSessionDto).ToList()
                };
            }

            var refreshToken = _jwtService.GenerateRefreshToken();
            var device = DeviceInfoParser.Parse(request.UserAgent);

            var refreshTokenEntity = new RefreshToken
            {
                EmployeeId = employee.Id,
                Token = refreshToken,
                JwtId = Guid.NewGuid().ToString(),
                CreatedAt = now,
                ExpiresAt = now.AddDays(30),
                Revoked = false,
                DeviceOs = device.Os,
                DeviceBrowser = device.Browser,
                IpAddress = request.IpAddress,
                LastSeenAt = now
            };

            // ابتدا ذخیره می‌شود تا Id (شناسه‌ی نشست) مشخص شود، سپس در توکن می‌آید.
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            var jwt = _jwtService.GenerateToken(
                employee.Id,
                employee.FirstName,
                employee.PersonnelNumber ?? "",
                employee.Role.ToString(),
                employee.IsSeniorAdmin,
                employee.ManagedLoanTypeId,
                refreshTokenEntity.Id);

            return new LoginResponseDto
            {
                AccessToken = jwt,
                RefreshToken = refreshToken,
                ExpireAt = now.AddMinutes(60)
            };
        }

        private static SessionDto ToSessionDto(RefreshToken t) => new()
        {
            Id = t.Id,
            DeviceOs = t.DeviceOs,
            DeviceBrowser = t.DeviceBrowser,
            IpAddress = t.IpAddress,
            LastSeenAt = t.LastSeenAt,
            CreatedAt = t.CreatedAt,
            IsCurrent = false
        };
    }
}