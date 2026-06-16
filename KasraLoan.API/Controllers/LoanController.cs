using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var result = await _loanRequestService.CreateLoanRequestAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new
                {
                    result.Message
                });

            return Ok(new
            {
                result.LoanRequestId,
                result.Message
            });
        }
    }
}