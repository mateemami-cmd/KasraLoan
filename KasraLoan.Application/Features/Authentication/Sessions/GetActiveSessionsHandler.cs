using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.Sessions
{
    public class GetActiveSessionsHandler
        : IRequestHandler<GetActiveSessionsQuery, GetActiveSessionsResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetActiveSessionsHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ICurrentUserService currentUserService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetActiveSessionsResponse> Handle(
            GetActiveSessionsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var currentSessionId = _currentUserService.SessionId;

            var sessions = await _refreshTokenRepository.GetActiveByEmployeeAsync(userId);

            // «آخرین دسترسی»ِ نشستِ جاری را همین‌جا تازه می‌کنیم چون کاربر فعال است.
            var current = sessions.FirstOrDefault(s => s.Id == currentSessionId);
            if (current != null)
            {
                current.LastSeenAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(current);
            }

            return new GetActiveSessionsResponse
            {
                Sessions = sessions.Select(s => new SessionDto
                {
                    Id = s.Id,
                    DeviceOs = s.DeviceOs,
                    DeviceBrowser = s.DeviceBrowser,
                    IpAddress = s.IpAddress,
                    LastSeenAt = s.LastSeenAt,
                    CreatedAt = s.CreatedAt,
                    IsCurrent = s.Id == currentSessionId
                }).ToList()
            };
        }
    }
}
