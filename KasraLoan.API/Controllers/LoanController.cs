using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KasraLoan.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanController : ControllerBase
    {
        private readonly ILoanRequestService _loanRequestService;
        //private readonly ILoanRequestService _loanService;

        public LoanController(ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService;
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
    }
}