using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Features.Employee.Commands.CreateEmployee;
using KasraLoan.Application.Features.Employee.Commands.DeleteEmployee;
using KasraLoan.Application.Features.Employee.Commands.RegenerateUsernames;
using KasraLoan.Application.Features.Employee.Commands.RestoreEmployee;
using KasraLoan.Application.Features.Employee.Commands.SetAccountStatus;
using KasraLoan.Application.Features.Employee.Commands.SetNationalId;
using KasraLoan.Application.Features.Employee.Commands.SetAdminScope;
using KasraLoan.Application.Features.Employee.Commands.GrantLoanPermission;
using KasraLoan.Application.Features.Employee.Commands.SetEmployeeScoreOverride;
using KasraLoan.Application.Features.Employee.Commands.SetEmploymentStatus;
using KasraLoan.Application.Features.Employee.Commands.UpdateEmployeeByAdmin;
using KasraLoan.Application.Features.Employee.Queries.GetAllEmployees;
using KasraLoan.Application.Features.Employee.Queries.GetNextIdentifier;
using KasraLoan.Application.Features.Employee.Queries.GetEmployeeById;
using KasraLoan.Application.Features.Employee.Queries.GetEmploymentStatusHistory;
using KasraLoan.Application.Features.Employee.Queries.GetEmployeeScore;
using KasraLoan.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KasraLoan.API.Controllers
{
    // نکته‌ی طراحی مهم: «حذف» در اینجا حذفِ نرم (soft delete) است، نه حذفِ فیزیکی.
    // ردیفِ کارمند و همه‌ی سوابقش (وام‌ها، اقساط، پرداخت‌ها) در دیتابیس می‌ماند چون
    // متعلق به شرکت است؛ فقط علامتِ IsDeleted می‌خورد و از فهرست‌های عادی کنار می‌رود.
    // قابلِ بازگردانی است (endpoint ‌restore).
    //
    // کلِ مدیریت کارمندان و ادمین‌ها فقط دستِ «ادمین ارشد» است؛ ادمین‌های وام
    // اینجا هیچ دسترسی‌ای ندارند.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = LoanPolicies.SeniorAdminOnly)]
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

        // نام کاربری همه‌ی کارمندان را بر اساس الگوریتم «سال+کد سمت+ترتیب» بازتولید می‌کند.
        // قطعی و idempotent است؛ ادمین‌ها دست‌نخورده می‌مانند.
        [HttpPost("regenerate-usernames")]
        public async Task<IActionResult> RegenerateUsernames()
        {
            var result = await _mediator.Send(new RegenerateUsernamesCommand());

            return Ok(result);
        }

        // پیش‌نمایشِ کد ۹ رقمیِ بعدی (نام کاربری = شماره‌ی پرسنلی) برای فرم افزودن کاربر.
        [HttpGet("next-identifier")]
        public async Task<IActionResult> GetNextIdentifier(
            [FromQuery] int jobPositionId,
            [FromQuery] DateTime hireDate)
        {
            var result = await _mediator.Send(new GetNextIdentifierQuery
            {
                JobPositionId = jobPositionId,
                HireDate = hireDate
            });

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

        // فعال/غیرفعال کردن حساب کاربری (دسترسی ورود). جدا از وضعیت اشتغال است:
        // حساب غیرفعال نمی‌تواند وارد شود و درخواست وام بدهد.
        [HttpPut("{employeeId}/account-status")]
        public async Task<IActionResult> SetAccountStatus(
            Guid employeeId,
            [FromBody] SetAccountStatusRequestDto request)
        {
            var result = await _mediator.Send(new SetAccountStatusCommand
            {
                EmployeeId = employeeId,
                Request = request
            });

            return Ok(result);
        }

        // حذفِ نرمِ کارمند: ردیف و سوابقش می‌ماند، فقط علامتِ حذف می‌خورد و از
        // فهرست‌های عادی کنار می‌رود. قابلِ بازگردانی.
        [HttpDelete("{employeeId}")]
        public async Task<IActionResult> Delete(Guid employeeId)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand { EmployeeId = employeeId });

            return Ok(result);
        }

        // تعیین/ویرایشِ کد ملی (برای جایگزینیِ مقادیرِ موقتِ کاربرانِ قدیمی).
        [HttpPut("{employeeId}/national-id")]
        public async Task<IActionResult> SetNationalId(
            Guid employeeId,
            [FromBody] SetNationalIdRequestDto request)
        {
            var result = await _mediator.Send(new SetNationalIdCommand
            {
                EmployeeId = employeeId,
                Request = request
            });

            return Ok(result);
        }

        // بازگرداندنِ کارمندِ حذف‌شده (به‌صورتِ غیرفعال).
        [HttpPost("{employeeId}/restore")]
        public async Task<IActionResult> Restore(Guid employeeId)
        {
            var result = await _mediator.Send(new RestoreEmployeeCommand { EmployeeId = employeeId });

            return Ok(result);
        }

        // «دسترسی‌ها»: سطح دسترسی یک ادمین را عوض می‌کند (ارشد یا ادمینِ یک نوع وام).
        [HttpPut("{employeeId}/admin-scope")]
        public async Task<IActionResult> SetAdminScope(
            Guid employeeId,
            [FromBody] SetAdminScopeRequestDto request)
        {
            var result = await _mediator.Send(new SetAdminScopeCommand
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