using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadHandler
        : IRequestHandler<MarkAllNotificationsAsReadCommand, MarkAllNotificationsAsReadResponse>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public MarkAllNotificationsAsReadHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<MarkAllNotificationsAsReadResponse> Handle(
            MarkAllNotificationsAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var employeeId = _currentUserService.UserId;

            var unread = await _notificationRepository.GetUnreadByEmployeeIdAsync(employeeId);

            foreach (var notification in unread)
            {
                notification.IsRead = true;
            }

            if (unread.Count > 0)
            {
                await _notificationRepository.SaveChangesAsync();
            }

            return new MarkAllNotificationsAsReadResponse
            {
                MarkedCount = unread.Count,
                Message = $"{unread.Count} اعلان به‌عنوان خوانده‌شده علامت خورد."
            };
        }
    }
}
