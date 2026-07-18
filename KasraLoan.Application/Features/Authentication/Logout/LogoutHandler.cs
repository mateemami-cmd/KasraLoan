using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LogoutHandler(
            IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            var refreshToken =
                await _refreshTokenRepository
                    .GetByTokenAsync(request.RefreshToken);

            if (refreshToken == null)
                throw new UnauthorizedAccessException("Refresh Token not found");

            refreshToken.Revoked = true;

            await _refreshTokenRepository.UpdateAsync(refreshToken);
        }
    }
}