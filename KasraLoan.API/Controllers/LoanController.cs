using Google.GenAI;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Features.Loan.Commands.ApproveLoan;
using KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest;
using KasraLoan.Application.Features.Loan.Commands.RejectLoan;
using KasraLoan.Application.Features.Loan.Queries.GetMyLoans;
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

        public LoanController(IMediator mediator,ILoanInstallmentService loanInstallmentService, ILoanRequestService loanRequestService)
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
        public async Task<IActionResult> GetAllLoans()
        {
            var result = await _loanRequestService.GetAllLoansAsync();

            if (!result.IsSuccess)
                return BadRequest(result.Message);

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
    }
}