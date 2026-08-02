using KasraLoan.Application.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsResponse
    {
        public List<NotificationDto> Items { get; set; } = new();

        public int UnreadCount { get; set; }
    }
}
