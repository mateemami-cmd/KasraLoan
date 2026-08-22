using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.Sessions
{
    public class RevokeSessionHandler
        : IRequestHandler<RevokeSessionCommand, RevokeSessionResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;

        public RevokeSessionHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ICurrentUserService currentUserService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _currentUserService = currentUserService;
        }

        public async Task<RevokeSessionResponse> Handle(
            RevokeSessionCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _refreshTokenRepository.GetByIdAsync(request.SessionId);

            // فقط نشستِ خودِ همین کاربر قابل قطع است.
            if (session == null || session.EmployeeId != _currentUserService.UserId)
                throw new KeyNotFoundException("نشست یافت نشد.");

            if (!session.Revoked)
            {
                session.Revoked = true;
                await _refreshTokenRepository.UpdateAsync(session);
            }

            return new RevokeSessionResponse { Message = "نشست قطع شد." };
        }
    }
}
