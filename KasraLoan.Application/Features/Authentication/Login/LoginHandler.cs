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

        // مهلتِ بی‌کاری (idle) برحسب دقیقه: تا وقتی داخل این بازه فعالیت باشد نشست
        // زنده می‌ماند و با هر فعالیت جلو می‌رود؛ اگر این‌قدر بی‌کار بماند منقضی می‌شود.
        private const int IdleMinutes = 10;

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
            var device = DeviceInfoParser.Parse(request.UserAgent);

            // شناسه‌ی دستگاه از کلاینت می‌آید. اگر نیامد (کلاینتِ قدیمی)، یک شناسه‌ی
            // یک‌بارمصرف می‌سازیم تا این ورود مثل یک دستگاهِ مستقل رفتار کند.
            var deviceId = string.IsNullOrWhiteSpace(request.LoginRequest.DeviceId)
                ? Guid.NewGuid().ToString("N")
                : request.LoginRequest.DeviceId!.Trim();

            var refreshToken = _jwtService.GenerateRefreshToken();

            // یکتاسازی بر اساس دستگاه: اگر نشستِ فعالی برای همین دستگاه هست، همان را
            // به‌روز می‌کنیم (نه ردیفِ جدید). پس صد بار ورود با یک دستگاه = یک نشست،
            // و هیچ سقفی روی تعداد دستگاه‌ها نیست.
            var session = await _refreshTokenRepository.GetActiveByEmployeeAndDeviceAsync(employee.Id, deviceId);

            if (session != null)
            {
                session.Token = refreshToken;
                session.JwtId = Guid.NewGuid().ToString();
                session.ExpiresAt = now.AddMinutes(IdleMinutes);
                session.LastSeenAt = now;
                session.DeviceOs = device.Os;
                session.DeviceBrowser = device.Browser;
                session.IpAddress = request.IpAddress;
                session.Revoked = false;
                await _refreshTokenRepository.UpdateAsync(session);
            }
            else
            {
                session = new RefreshToken
                {
                    EmployeeId = employee.Id,
                    Token = refreshToken,
                    JwtId = Guid.NewGuid().ToString(),
                    CreatedAt = now,
                    ExpiresAt = now.AddMinutes(IdleMinutes),
                    Revoked = false,
                    DeviceId = deviceId,
                    DeviceOs = device.Os,
                    DeviceBrowser = device.Browser,
                    IpAddress = request.IpAddress,
                    LastSeenAt = now
                };

                // ابتدا ذخیره می‌شود تا Id (شناسه‌ی نشست) مشخص شود، سپس در توکن می‌آید.
                await _refreshTokenRepository.AddAsync(session);
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