using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Notifications.Queries.GetUnreadNotificationCount
{
    public class GetUnreadNotificationCountQuery : IRequest<GetUnreadNotificationCountResponse>
    {
    }
}
