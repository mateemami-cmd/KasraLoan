using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.GetLoginHistory
{
    public class GetLoginHistoryHandler
        : IRequestHandler<GetLoginHistoryQuery, GetLoginHistoryResponse>
    {
        // تعداد ردیف‌های نمایش‌داده‌شده در «تاریخچه ورودهای اخیر».
        private const int RecentCount = 3;

        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLoginHistoryHandler(
            ILoginHistoryRepository loginHistoryRepository,
            ICurrentUserService currentUserService)
        {
            _loginHistoryRepository = loginHistoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetLoginHistoryResponse> Handle(
            GetLoginHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var entries = await _loginHistoryRepository.GetRecentByEmployeeAsync(
                _currentUserService.UserId, RecentCount);

            return new GetLoginHistoryResponse
            {
                History = entries.Select(x => new LoginHistoryDto
                {
                    AttemptedAt = x.AttemptedAt,
                    IpAddress = x.IpAddress,
                    DeviceOs = x.DeviceOs,
                    DeviceBrowser = x.DeviceBrowser,
                    IsSuccess = x.IsSuccess
                }).ToList()
            };
        }
    }
}
