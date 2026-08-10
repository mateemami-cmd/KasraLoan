using KasraLoan.Application.DTOs.JobPositions;
using KasraLoan.Application.Features.JobPositions.Commands.CreateJobPosition;
using KasraLoan.Application.Features.JobPositions.Commands.UpdateJobPosition;
using KasraLoan.Application.Features.JobPositions.Queries.GetAllJobPositions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobPositionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobPositionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// لیست سمت‌های شغلی. برای پر کردن فرم افزودن/ویرایش کارمند لازم است،
        /// پس برای هر کاربر واردشده‌ای در دسترس است.
        /// با activeOnly=true فقط سمت‌های فعال برمی‌گردند.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var result = await _mediator.Send(new GetAllJobPositionsQuery
            {
                ActiveOnly = activeOnly
            });

            return Ok(result);
        }

        /// <summary>ساخت سمت شغلی جدید. فقط ادمین.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SaveJobPositionRequestDto request)
        {
            var result = await _mediator.Send(new CreateJobPositionCommand
            {
                Request = request
            });

            return Ok(result);
        }

        /// <summary>ویرایش سمت شغلی (عنوان، حقوق پایه، فعال/غیرفعال). فقط ادمین.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveJobPositionRequestDto request)
        {
            var result = await _mediator.Send(new UpdateJobPositionCommand
            {
                Id = id,
                Request = request
            });

            return Ok(result);
        }
    }
}
