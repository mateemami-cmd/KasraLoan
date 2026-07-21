using Google.GenAI;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Features.Loan.Commands.ApproveLoan;
using KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest;
using KasraLoan.Application.Features.Loan.Commands.RejectLoan;
using KasraLoan.Application.Features.Loan.Commands.UploadLoanDocument;
using KasraLoan.Application.Features.Loan.Queries.GetAdminDashboard;
using KasraLoan.Application.Features.Loan.Queries.GetLoanById;
using KasraLoan.Application.Features.Loan.Queries.GetLoanDocuments;
using KasraLoan.Application.Features.Loan.Queries.GetMyLoans;
using KasraLoan.Application.Features.Loan.Queries.GetMyLoans.GetAllLoans;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
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
        private readonly ILoanRequestService _loanRequestService;
        //private readonly ILoanRequestService _loanService;

        public LoanController(IMediator mediator, ILoanInstallmentService loanInstallmentService, ILoanRequestService loanRequestService)
        {
            _mediator = mediator;
            _loanInstallmentService = loanInstallmentService;
            _loanRequestService = loanRequestService;
        }

        [HttpPost("request")]
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
        public async Task<IActionResult> RejectLoan(Guid id)
        {
            var result = await _mediator.Send(new RejectLoanCommand
            {
                LoanRequestId = id
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

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminLoans([FromQuery] LoanStatus? status)
        {
            var result = await _loanRequestService.GetAdminLoansAsync(status);

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