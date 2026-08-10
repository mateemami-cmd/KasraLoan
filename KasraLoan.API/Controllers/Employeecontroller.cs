using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Features.Employee.Commands.CreateEmployee;
using KasraLoan.Application.Features.Employee.Commands.GrantLoanPermission;
using KasraLoan.Application.Features.Employee.Commands.SetEmployeeScoreOverride;
using KasraLoan.Application.Features.Employee.Commands.SetEmploymentStatus;
using KasraLoan.Application.Features.Employee.Commands.UpdateEmployeeByAdmin;
using KasraLoan.Application.Features.Employee.Queries.GetAllEmployees;
using KasraLoan.Application.Features.Employee.Queries.GetEmployeeById;
using KasraLoan.Application.Features.Employee.Queries.GetEmploymentStatusHistory;
using KasraLoan.Application.Features.Employee.Queries.GetEmployeeScore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KasraLoan.API.Controllers
{
    // نکته‌ی طراحی مهم: در این کنترلر عمداً هیچ اندپوینت DELETE وجود ندارد.
    // این سیستم فقط برای مدیریت وام است، نه مدیریت کامل پرسنل شرکت؛
    // بنابراین ادمین می‌تواند کارمندان را ویرایش یا غیرفعال کند، اما حذف کامل مجاز نیست.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequestDto request)
        {
            var result = await _mediator.Send(new CreateEmployeeCommand
            {
                Request = request
            });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllEmployeesQuery());

            return Ok(result);
        }

        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetById(Guid employeeId)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery
            {
                EmployeeId = employeeId
            });

            return Ok(result);
        }

        [HttpPut("{employeeId}")]
        public async Task<IActionResult> Update(
            Guid employeeId,
            [FromBody] AdminUpdateEmployeeRequestDto request)
        {
            var result = await _mediator.Send(new UpdateEmployeeByAdminCommand
            {
                EmployeeId = employeeId,
                Request = request
            });

            return Ok(result);
        }

        [HttpGet("{employeeId}/score")]
        public async Task<IActionResult> GetScore(Guid employeeId)
        {
            var result = await _mediator.Send(new GetEmployeeScoreQuery
            {
                EmployeeId = employeeId
            });

            return Ok(result);
        }

        [HttpPut("{employeeId}/score")]
        public async Task<IActionResult> SetScoreOverride(
            Guid employeeId,
            [FromBody] SetEmployeeScoreOverrideRequestDto request)
        {
            var result = await _mediator.Send(new SetEmployeeScoreOverrideCommand
            {
                EmployeeId = employeeId,
                Request = request
            });

            return Ok(result);
        }

        /// <summary>
        /// تغییر وضعیت اشتغال (مشغول به کار / پایان همکاری).
        ///
        /// عمداً اندپوینت جداست و در PUT معمولیِ ویرایش کارمند نیست: یک رویداد مالی
        /// است، پنجره‌ی زمانی دارد (غیرفعال کردن فقط نزدیک قطعی‌شدن لیست حقوق) و
        /// لاگ و تاریخچه ثبت می‌کند. نباید به‌عنوان عارضه‌ی جانبیِ ویرایش پروفایل رخ دهد.
        /// </summary>
        [HttpPut("{employeeId}/employment-status")]
        public async Task<IActionResult> SetEmploymentStatus(
            Guid employeeId,
            [FromBody] SetEmploymentStatusRequestDto request)
        {
            var result = await _mediator.Send(new SetEmploymentStatusCommand
            {
                EmployeeId = employeeId,
                Request = request
            });

            return Ok(result);
        }

        /// <summary>تاریخچه‌ی تغییرات وضعیت اشتغال یک کارمند.</summary>
        [HttpGet("{employeeId}/employment-status/history")]
        public async Task<IActionResult> GetEmploymentStatusHistory(Guid employeeId)
        {
            var result = await _mediator.Send(new GetEmploymentStatusHistoryQuery
            {
                EmployeeId = employeeId
            });

            return Ok(result);
        }

        /// <summary>
        /// پنل جدا و مستقل از ویرایش پروفایل: اعطا یا لغوِ مجوز یک‌بارمصرفِ
        /// درخواست وام به یک یا چند کارمند خاص، بدون تغییر امتیاز واقعی‌شان.
        /// </summary>
        [HttpPut("loan-permission")]
        public async Task<IActionResult> GrantLoanPermission(
            [FromBody] GrantLoanPermissionRequestDto request)
        {
            var result = await _mediator.Send(new GrantLoanPermissionCommand
            {
                Request = request
            });

            return Ok(result);
        }
    }
}