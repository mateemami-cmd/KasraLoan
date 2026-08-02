using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Queries.GetUnreadNotificationCount
{
    public class GetUnreadNotificationCountHandler
        : IRequestHandler<GetUnreadNotificationCountQuery, GetUnreadNotificationCountResponse>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUnreadNotificationCountHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetUnreadNotificationCountResponse> Handle(
            GetUnreadNotificationCountQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = _currentUserService.UserId;

            var count = await _notificationRepository.GetUnreadCountAsync(employeeId);

            return new GetUnreadNotificationCountResponse
            {
                UnreadCount = count
            };
        }
    }
}
