using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsQuery : IRequest<GetMyNotificationsResponse>
    {
        /// <summary>اگر true باشد فقط اعلان‌های خوانده‌نشده برمی‌گردند.</summary>
        public bool UnreadOnly { get; set; }
    }
}
