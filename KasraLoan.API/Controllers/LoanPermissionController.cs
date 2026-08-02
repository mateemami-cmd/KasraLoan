using KasraLoan.Application.DTOs.LoanPermission;
using KasraLoan.Application.Features.LoanPermission.Commands.ApproveLoanPermissionRequest;
using KasraLoan.Application.Features.LoanPermission.Commands.CreateLoanPermissionRequest;
using KasraLoan.Application.Features.LoanPermission.Commands.RejectLoanPermissionRequest;
using KasraLoan.Application.Features.LoanPermission.Queries.GetAllLoanPermissionRequests;
using KasraLoan.Application.Features.LoanPermission.Queries.GetMyLoanPermissionRequests;
using KasraLoan.Domain.Enums;
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
    public class LoanPermissionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanPermissionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// ثبت درخواست مجوز وام توسط کارمند: نوع وام و دلیل را می‌فرستد تا ادمین بررسی کند.
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateLoanPermissionRequestDto request)
        {
            var result = await _mediator.Send(new CreateLoanPermissionRequestCommand
            {
                Request = request
            });

            return Ok(result);
        }

        /// <summary>
        /// لیست درخواست‌های مجوزِ کارمندِ فعلی، همراه با وضعیت هر کدام.
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var result = await _mediator.Send(new GetMyLoanPermissionRequestsQuery());

            return Ok(result);
        }

        /// <summary>
        /// لیست کامل درخواست‌های مجوز برای ادمین، با صفحه‌بندی و فیلتر وضعیت.
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] LoanPermissionRequestStatus? status = null)
        {
            var result = await _mediator.Send(new GetAllLoanPermissionRequestsQuery
            {
                Page = page,
                PageSize = pageSize,
                Status = status
            });

            return Ok(result);
        }

        /// <summary>
        /// تأیید درخواست مجوز توسط ادمین: مجوز یک‌بارمصرف برای کارمند فعال می‌شود.
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _mediator.Send(new ApproveLoanPermissionRequestCommand
            {
                PermissionRequestId = id
            });

            return Ok(result);
        }

        /// <summary>
        /// رد درخواست مجوز توسط ادمین، همراه با دلیل اختیاری.
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLoanPermissionRequestDto request)
        {
            var result = await _mediator.Send(new RejectLoanPermissionRequestCommand
            {
                PermissionRequestId = id,
                AdminResponse = request?.AdminResponse
            });

            return Ok(result);
        }
    }
}
