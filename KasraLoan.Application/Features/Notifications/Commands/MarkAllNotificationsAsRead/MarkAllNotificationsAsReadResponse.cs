using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadResponse
    {
        public int MarkedCount { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
