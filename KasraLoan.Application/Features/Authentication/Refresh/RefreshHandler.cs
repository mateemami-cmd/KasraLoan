using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Refresh
{
    public class RefreshHandler : IRequestHandler<RefreshCommand, LoginResponseDto>
    {

        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;

        public RefreshHandler(IRefreshTokenRepository refreshTokenRepository, IJwtService jwtService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
        }

        // باید با IdleMinutes در LoginHandler یکی باشد: مهلتِ بی‌کاریِ نشست.
        private const int IdleMinutes = 10;

        public async Task<LoginResponseDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.Request.RefreshToken);

            if (refreshToken == null)
                throw new UnauthorizedAccessException("Invalid Refresh Token");

            if (refreshToken.Revoked)
                throw new UnauthorizedAccessException("Refresh Token Revoked");

            // اگر بیش از مهلتِ idle بی‌کار مانده، منقضی است: کاربر باید دوباره وارد شود
            // و این نشست از «نشست‌های فعال» خودبه‌خود حذف می‌شود.
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh Token Expired");

            var employee = refreshToken.Employee;
            var now = DateTime.UtcNow;

            // همان نشست (همان ردیف و همان DeviceId) را جلو می‌بریم: فقط مهلتِ idle و
            // «آخرین دسترسی» به‌روز می‌شود. توکنِ رفرش را نمی‌چرخانیم تا چند تبِ هم‌زمان
            // که یک رفرش‌توکن مشترک دارند با هم تداخل نکنند.
            refreshToken.ExpiresAt = now.AddMinutes(IdleMinutes);
            refreshToken.LastSeenAt = now;
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            var accessToken = _jwtService.GenerateToken(
                employee.Id,
                employee.FirstName,
                employee.PersonnelNumber ?? "",
                employee.Role.ToString(),
                employee.IsSeniorAdmin,
                employee.ManagedLoanTypeId,
                refreshToken.Id);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpireAt = now.AddMinutes(IdleMinutes)
            };
        }
    }
}