using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using KasraLoan.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Google.GenAI;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanController : ControllerBase
    {
        private readonly ILoanRequestService _loanRequestService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        //private readonly ILoanRequestService _loanService;

        public LoanController(ILoanRequestService loanRequestService, ILoanInstallmentService loanInstallmentService)
        {
            _loanRequestService = loanRequestService;
            _loanInstallmentService = loanInstallmentService;
        }

        //public LoanController(ILoanRequestService loanService)
        //{
        //    _loanRequestService = loanService;
        //}

        [HttpPost("request")]
        public async Task<IActionResult> CreateLoanRequest(CreateLoanRequestDto dto)
        {
            var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _loanRequestService.CreateLoanRequestAsync(employeeId, dto);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    result.Message
                });

            return Ok(new
            {
                LoanRequestId = result.Data,
                result.Message
            });
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveLoan(Guid id)
        {
            var result = await _loanRequestService.ApproveLoanAsync(id);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectLoan(Guid id)
        {
            var result = await _loanRequestService.RejectLoanAsync(id);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpGet("my-loans")]
        [Authorize]
        public async Task<IActionResult> GetMyLoans()
        {
            var employeeId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _loanRequestService.GetLoansByEmployeeIdAsync(Guid.Parse(employeeId));

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