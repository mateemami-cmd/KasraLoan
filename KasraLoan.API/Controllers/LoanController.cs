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

        public LoanController(ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService;
        }

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
    }
}