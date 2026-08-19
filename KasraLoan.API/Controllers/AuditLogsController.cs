using KasraLoan.Application.Features.AuditLogs.Queries.GetAuditLogs;
using KasraLoan.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KasraLoan.API.Controllers
{
    // گزارش‌های سیستم فقط برای ادمین ارشد.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = LoanPolicies.SeniorAdminOnly)]
    public class AuditLogsController : ControllerBase
    {
        private readonly IMediator _mediator;


        public AuditLogsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAuditLogsQuery());

            return Ok(result);
        }
    }
}