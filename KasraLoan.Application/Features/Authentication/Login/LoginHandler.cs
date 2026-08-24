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
        private readonly ILoginHistoryRepository _loginHistoryRepository;

        public LoginHandler(IEmployeeRepository employeeRepository, IPasswordHasher passwordHasher, IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository, ILoginHistoryRepository loginHistoryRepository)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _loginHistoryRepository = loginHistoryRepository;
        }

        // مهلتِ بی‌کاری (idle) برحسب دقیقه: تا وقتی داخل این بازه فعالیت باشد نشست
        // زنده می‌ماند و با هر فعالیت جلو می‌رود؛ اگر این‌قدر بی‌کار بماند منقضی می‌شود.
        private const int IdleMinutes = 10;

        // حداکثر نشستِ فعالِ هم‌زمان: فقط ۴ نشستِ اخیر نگه داشته می‌شود.
        private const int MaxActiveSessions = 4;

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(request.LoginRequest.Username);

            const string invalidCredentialsMessage = "نام کاربری یا رمز عبور اشتباه است.";

            // نامِ کاربریِ اشتباه: کاربری برای نسبت‌دادن وجود ندارد، پس تاریخچه ثبت نمی‌شود.
            if (employee == null)
                throw new UnauthorizedAccessException(invalidCredentialsMessage);

            var now = DateTime.UtcNow;
            var device = DeviceInfoParser.Parse(request.UserAgent);

            // نتیجه‌ی این تلاش = رمزِ درست و حسابِ فعال. در هر حال (موفق یا ناموفق) در
            // «تاریخچه ورودها» ثبت می‌شود تا ستونِ «نتیجه» معنی داشته باشد.
            var isPasswordValid = _passwordHasher.Verify(request.LoginRequest.Password, employee.PasswordHash);
            var loginSucceeded = employee.IsActive && isPasswordValid;

            await _loginHistoryRepository.AddAsync(new LoginHistory
            {
                EmployeeId = employee.Id,
                AttemptedAt = now,
                IpAddress = request.IpAddress,
                DeviceOs = device.Os,
                DeviceBrowser = device.Browser,
                IsSuccess = loginSucceeded
            });

            if (!loginSucceeded)
                throw new UnauthorizedAccessException(invalidCredentialsMessage);

            // شناسه‌ی دستگاه از کلاینت می‌آید. اگر نیامد (کلاینتِ قدیمی)، یک شناسه‌ی
            // یک‌بارمصرف می‌سازیم تا این ورود مثل یک دستگاهِ مستقل رفتار کند.
            var deviceId = string.IsNullOrWhiteSpace(request.LoginRequest.DeviceId)
                ? Guid.NewGuid().ToString("N")
                : request.LoginRequest.DeviceId!.Trim();

            var refreshToken = _jwtService.GenerateRefreshToken();

            // هر ورود یک نشستِ فعالِ جدید می‌سازد (بدونِ یکتاسازیِ per-device). پس اگر
            // صفحه را ببندی و بعد دوباره وارد شوی، یک نشستِ فعالِ جدید ساخته می‌شود؛
            // نشستِ قبلی هم اگر تبش بسته بماند، بعد از مهلتِ بی‌کاری خودش منقضی می‌شود.
            // (DeviceId فقط برای نمایش نگه داشته می‌شود، نه برای یکتاسازی.)
            var session = new RefreshToken
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

            // سقفِ نشست‌های فعال: پنجره‌ی غلتانِ ۴تایی. با ساختِ نشستِ جدید، اگر تعداد
            // از ۴ بیشتر شد، قدیمی‌ترین‌ها باطل می‌شوند تا همیشه فقط ۴ نشستِ اخیر بماند.
            // (GetActiveByEmployeeAsync از جدید به قدیم مرتب است، پس از ایندکس ۴ به بعد
            // قدیمی‌ترها هستند.)
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