using KasraLoan.Application.DTOs.LoanTypes;
using KasraLoan.Application.Features.LoanTypes.Commands.SetLoanTypeActiveStatus;
using KasraLoan.Application.Features.LoanTypes.Queries.GetAllLoanTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanTypeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanTypeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// لیست انواع وام. کارمند و ادمین هر دو می‌بینند؛ وضعیت فعال/غیرفعال هر وام مشخص است.
        /// با activeOnly=true فقط وام‌های فعال برمی‌گردند.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var result = await _mediator.Send(new GetAllLoanTypesQuery
            {
                ActiveOnly = activeOnly
            });

            return Ok(result);
        }

        /// <summary>
        /// فعال یا غیرفعال کردن یک نوع وام توسط ادمین.
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetLoanTypeActiveStatusRequestDto request)
        {
            var result = await _mediator.Send(new SetLoanTypeActiveStatusCommand
            {
                LoanTypeId = id,
                IsActive = request.IsActive
            });

            return Ok(result);
        }
    }
}
