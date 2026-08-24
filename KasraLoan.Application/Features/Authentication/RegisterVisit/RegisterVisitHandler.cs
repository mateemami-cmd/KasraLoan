using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common;
using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.RegisterVisit
{
    public class RegisterVisitHandler : IRequestHandler<RegisterVisitCommand, LoginResponseDto>
    {
        // باید با LoginHandler یکی باشد.
        private const int IdleMinutes = 10;
        private const int MaxActiveSessions = 4;

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public RegisterVisitHandler(
            IEmployeeRepository employeeRepository,
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            ILoginHistoryRepository loginHistoryRepository,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _loginHistoryRepository = loginHistoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task<LoginResponseDto> Handle(RegisterVisitCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null || !employee.IsActive)
                throw new UnauthorizedAccessException("کاربر معتبر نیست.");

            var now = DateTime.UtcNow;
            var device = DeviceInfoParser.Parse(request.UserAgent);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // نشستِ فعالِ جدید برای این تب.
            var session = new RefreshToken
            {
                EmployeeId = employee.Id,
                Token = refreshToken,
                JwtId = Guid.NewGuid().ToString(),
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(IdleMinutes),
                Revoked = false,
                DeviceId = Guid.NewGuid().ToString("N"),
                DeviceOs = device.Os,
                DeviceBrowser = device.Browser,
                IpAddress = request.IpAddress,
                LastSeenAt = now
            };
            await _refreshTokenRepository.AddAsync(session);

            // یک ردیفِ تاریخچه‌ی ورود (موفق، چون با نشستِ معتبر وارد شده).
            await _loginHistoryRepository.AddAsync(new LoginHistory
            {
                EmployeeId = employee.Id,
                AttemptedAt = now,
                IpAddress = request.IpAddress,
                DeviceOs = device.Os,
                DeviceBrowser = device.Browser,
                IsSuccess = true
            });

            // سقفِ ۴ نشستِ فعال (پنجره‌ی غلتان): قدیمی‌ترها باطل می‌شوند.
            var activeSessions = await _refreshTokenRepository.GetActiveByEmployeeAsync(employee.Id);
            foreach (var old in activeSessions.Skip(MaxActiveSessions))
            {
                old.Revoked = true;
                await _refreshTokenRepository.UpdateAsync(old);
            }

            var jwt = _jwtService.GenerateToken(
                employee.Id,
                employee.FirstName,
                employee.PersonnelNumber ?? "",
                employee.Role.ToString(),
                employee.IsSeniorAdmin,
                employee.ManagedLoanTypeId,
                session.Id);

            return new LoginResponseDto
            {
                AccessToken = jwt,
                RefreshToken = refreshToken,
                ExpireAt = now.AddMinutes(IdleMinutes)
            };
        }
    }
}
