using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Features.Loan.Commands.ApproveLoan;
using KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest;
using KasraLoan.Application.Features.Loan.Commands.RejectLoan;
using KasraLoan.Application.Features.Loan.Commands.UploadLoanDocument;
using KasraLoan.Application.Features.Loan.Queries.GetAdminDashboard;
using KasraLoan.Application.Features.Loan.Queries.GetLoanById;
using KasraLoan.Application.Features.Loan.Queries.GetLoanDocuments;
using KasraLoan.Application.Features.Loan.Queries.GetLoanOutstanding;
using KasraLoan.Application.Features.Loan.Queries.GetLoanQuote;
using KasraLoan.API.Models;
using KasraLoan.Application.Features.Loan.Queries.GetMyLoans;
using KasraLoan.Application.Features.Loan.Queries.GetMyLoans.GetAllLoans;
using KasraLoan.Application.Features.Loan.Queries.GetRequestPool;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.API.Authorization;
using KasraLoan.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILoanInstallmentService _loanInstallmentService;

        public LoanController(IMediator mediator, ILoanInstallmentService loanInstallmentService)
        {
            _mediator = mediator;
            _loanInstallmentService = loanInstallmentService;
        }

        /// <summary>
        /// اطلاعات لازم برای پر کردن فرم درخواست وام: سقف، گزینه‌های مبلغ، و —
        /// اگر مبلغ داده شود — گزینه‌های تعداد اقساط با مبلغ ماهانه‌ی هرکدام.
        ///
        /// همه‌ی محاسبات سمت سرور انجام می‌شود تا فرم فرمول کارمزد و سقف را تکرار نکند.
        /// </summary>
        [HttpGet("quote")]
        public async Task<IActionResult> GetQuote(
            [FromQuery] int loanTypeId,
            [FromQuery] long? amount = null)
        {
            var result = await _mediator.Send(new GetLoanQuoteQuery
            {
                LoanTypeId = loanTypeId,
                Amount = amount
            });

            return Ok(result);
        }

        /// <summary>
        /// ثبت درخواست وام همراه با مدارک، در یک درخواست multipart.
        ///
        /// مدارک عمداً همین‌جا گرفته می‌شوند و نه در اندپوینت جدا: برای وام‌هایی
        /// که مدرک لازم دارند، درخواستِ بدون مدرک اصلاً نباید ساخته شود.
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> CreateLoanRequestWithFiles(
            [FromForm] CreateLoanRequestForm form)
        {
            var command = new CreateLoanRequestCommand
            {
                Request = new CreateLoanRequestDto
                {
                    LoanTypeId = form.LoanTypeId,
                    RequestedAmount = form.RequestedAmount,
                    InstallmentCount = form.InstallmentCount,
                    Travel = form.HasTravelDetails
                        ? new TravelDetailsDto
                        {
                            DestinationType = form.DestinationType ?? string.Empty,
                            Destination = form.Destination ?? string.Empty,
                            StartDate = form.StartDate ?? default,
                            EndDate = form.EndDate ?? default,
                            Notes = form.Notes
                        }
                        : null,
                    Marriage = form.HasMarriageDetails
                        ? new MarriageDetailsDto
                        {
                            MarriageDate = form.MarriageDate,
                            SpouseFirstName = form.SpouseFirstName ?? string.Empty,
                            SpouseLastName = form.SpouseLastName ?? string.Empty,
                            SpouseNationalId = form.SpouseNationalId ?? string.Empty,
                            Notes = form.Notes
                        }
                        : null,
                    SpecialCase = form.HasSpecialCaseDetails
                        ? new SpecialCaseDetailsDto
                        {
                            Category = form.SpecialCaseCategory ?? string.Empty,
                            Description = form.SpecialCaseDescription ?? string.Empty
                        }
                        : null,
                    ImmediatePayment = form.HasImmediatePaymentDetails
                        ? new ImmediatePaymentDetailsDto
                        {
                            Purpose = form.ImmediatePaymentPurpose ?? string.Empty
                        }
                        : null
                }
            };

            foreach (var file in form.Files ?? new List<IFormFile>())
            {
                if (file.Length == 0)
                    continue;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                command.Attachments.Add(new LoanAttachment
                {
                    Content = stream.ToArray(),
                    FileName = file.FileName,
                    ContentType = file.ContentType
                });
            }

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>نسخه‌ی JSON برای انواع وامی که مدرک و فرم اختصاصی ندارند.</summary>
        [HttpPost("request-json")]
        public async Task<IActionResult> CreateLoanRequest(CreateLoanRequestCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("{loanId}/upload-document")]
        [Authorize]
        public async Task<IActionResult> UploadDocument(Guid loanId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایلی انتخاب نشده است.");

            using var memoryStream = new MemoryStream();

            await file.CopyToAsync(memoryStream);

            var command = new UploadLoanDocumentCommand
            {
                LoanRequestId = loanId,
                FileContent = memoryStream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpGet("{loanId}/documents")]
        [Authorize]
        public async Task<IActionResult> GetLoanDocuments(Guid loanId)
        {
            var result = await _mediator.Send(new GetLoanDocumentsQuery
            {
                LoanRequestId = loanId
            });

            return Ok(result);
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveLoan(Guid id)
        {
            var result = await _mediator.Send(new ApproveLoanCommand
            {
                LoanRequestId = id
            });

            return Ok(result);
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectLoan(Guid id, [FromBody] RejectLoanRequestDto? body = null)
        {
            var result = await _mediator.Send(new RejectLoanCommand
            {
                LoanRequestId = id,
                RejectReason = body?.RejectReason
            });

            return Ok(result);
        }

        [HttpGet("my-loans")]
        [Authorize]
        public async Task<IActionResult> GetMyLoans()
        {
            var result = await _mediator.Send(new GetMyLoansQuery());

            return Ok(result);
        }

        /// <summary>
        /// لیست کامل وام‌ها برای ادمین، با صفحه‌بندی، فیلتر وضعیت و جست‌وجو.
        /// (این تنها اندپوینت لیست وام‌های ادمین است؛ نسخه‌ی موازی و ناقص‌تر قبلی حذف شد.)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLoans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] LoanStatus? status = null,
        [FromQuery] string? search = null)
        {
            var result = await _mediator.Send(new GetAllLoansQuery
            {
                Page = page,
                PageSize = pageSize,
                Status = status,
                Search = search
            });

            return Ok(result);
        }

        /// <summary>
        /// «استخرِ درخواست‌ها»: نمای یکپارچه‌ی همه‌ی درخواست‌های کارمندان
        /// (وام + مجوز وام) در یک جا. فقط ادمین ارشد.
        /// </summary>
        [HttpGet("requests/pool")]
        [Authorize(Policy = LoanPolicies.SeniorAdminOnly)]
        public async Task<IActionResult> GetRequestPool()
        {
            var result = await _mediator.Send(new GetRequestPoolQuery());

            return Ok(result);
        }

        /// <summary>
        /// مانده‌ی وام: چقدر پرداخت شده، چقدر مانده، و اگر تسویه‌ی یکجا مطالبه
        /// شده باشد مبلغ و مهلتش. مانده همیشه از روی اقساط پرداخت‌نشده حساب می‌شود.
        /// </summary>
        [HttpGet("{loanId}/outstanding")]
        public async Task<IActionResult> GetOutstanding(Guid loanId)
        {
            var result = await _mediator.Send(new GetLoanOutstandingQuery
            {
                LoanRequestId = loanId
            });

            return Ok(result);
        }

        [HttpGet("{loanId}/installments")]
        public async Task<IActionResult> GetInstallments(Guid loanId)
        {
            var result = await _loanInstallmentService.GetLoanInstallmentsAsync(loanId);

            return Ok(result);
        }

        [HttpPost("installments/{installmentId}/pay")]
        public async Task<IActionResult> PayInstallment(Guid installmentId)
        {
            var employeeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _loanInstallmentService.PayInstallmentAsync(installmentId, employeeId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetAdminDashboardQuery());

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetLoanById(Guid id)
        {
            var result = await _mediator.Send(new GetLoanByIdQuery
            {
                LoanId = id
            });

            return Ok(result);
        }
    }
}