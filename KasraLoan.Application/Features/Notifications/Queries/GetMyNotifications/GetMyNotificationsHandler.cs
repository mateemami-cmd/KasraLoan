using KasraLoan.Application.DTOs.Notifications;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsHandler
        : IRequestHandler<GetMyNotificationsQuery, GetMyNotificationsResponse>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyNotificationsHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetMyNotificationsResponse> Handle(
            GetMyNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = _currentUserService.UserId;

            var notifications = request.UnreadOnly
                ? await _notificationRepository.GetUnreadByEmployeeIdAsync(employeeId)
                : await _notificationRepository.GetByEmployeeIdAsync(employeeId);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(employeeId);

            var items = notifications.Select(x => new NotificationDto
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
                .ToList();

            return new GetMyNotificationsResponse
            {
                Items = items,
                UnreadCount = unreadCount
            };
        }
    }
}
