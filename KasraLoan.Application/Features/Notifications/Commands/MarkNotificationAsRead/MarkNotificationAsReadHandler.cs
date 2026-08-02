using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Commands.MarkNotificationAsRead
{
    public class MarkNotificationAsReadHandler
        : IRequestHandler<MarkNotificationAsReadCommand, MarkNotificationAsReadResponse>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public MarkNotificationAsReadHandler(
            INotificationRepository notificationRepository,
            ICurrentUserService currentUserService)
        {
            _notificationRepository = notificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<MarkNotificationAsReadResponse> Handle(
            MarkNotificationAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found");

            // کاربر فقط اجازه دارد اعلان‌های خودش را بخواند/علامت بزند.
            if (notification.EmployeeId != _currentUserService.UserId)
                throw new ForbiddenAccessException();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.SaveChangesAsync();
            }

            return new MarkNotificationAsReadResponse
            {
                Message = "اعلان به‌عنوان خوانده‌شده علامت خورد."
            };
        }
    }
}
