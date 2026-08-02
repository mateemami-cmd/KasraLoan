using KasraLoan.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;
using KasraLoan.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using KasraLoan.Application.Features.Notifications.Queries.GetMyNotifications;
using KasraLoan.Application.Features.Notifications.Queries.GetUnreadNotificationCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// اعلان‌های کاربرِ فعلی به‌همراه تعداد خوانده‌نشده‌ها.
        /// با unreadOnly=true فقط اعلان‌های خوانده‌نشده برمی‌گردند.
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMy([FromQuery] bool unreadOnly = false)
        {
            var result = await _mediator.Send(new GetMyNotificationsQuery
            {
                UnreadOnly = unreadOnly
            });

            return Ok(result);
        }

        /// <summary>تعداد اعلان‌های خوانده‌نشده (برای نمایش بَج زنگوله).</summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _mediator.Send(new GetUnreadNotificationCountQuery());

            return Ok(result);
        }

        /// <summary>علامت‌زدن یک اعلان به‌عنوان خوانده‌شده (فقط اعلان‌های خودِ کاربر).</summary>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _mediator.Send(new MarkNotificationAsReadCommand
            {
                NotificationId = id
            });

            return Ok(result);
        }

        /// <summary>علامت‌زدن همه‌ی اعلان‌های خوانده‌نشده‌ی کاربر به‌عنوان خوانده‌شده.</summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _mediator.Send(new MarkAllNotificationsAsReadCommand());

            return Ok(result);
        }
    }
}
