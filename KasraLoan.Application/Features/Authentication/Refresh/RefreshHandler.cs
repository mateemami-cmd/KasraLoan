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
    public class RefreshHandler
        : IRequestHandler<RefreshCommand, LoginResponseDto>
    {

        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;

        public RefreshHandler(IRefreshTokenRepository refreshTokenRepository, IJwtService jwtService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto> Handle(
    RefreshCommand request,
    CancellationToken cancellationToken)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.Request.RefreshToken);

            if (refreshToken == null)
                throw new UnauthorizedAccessException("Invalid Refresh Token");

            if (refreshToken.Revoked)
                throw new UnauthorizedAccessException("Refresh Token Revoked");

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh Token Expired");

            var employee = refreshToken.Employee;

            var accessToken = _jwtService.GenerateToken(
                employee.Id,
                employee.FirstName,
                employee.PersonnelNumber ?? "",
                employee.Role.ToString());

            var newRefreshToken = Guid.NewGuid().ToString("N");

            refreshToken.Revoked = true;

            await _refreshTokenRepository.UpdateAsync(refreshToken);

            await _refreshTokenRepository.AddAsync(new Domain.Entities.RefreshToken
            {
                EmployeeId = employee.Id,
                Token = newRefreshToken,
                JwtId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Revoked = false
            });

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(60)
            };
        }
    }
}