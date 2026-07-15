using MediatR;
using Microsoft.AspNetCore.Mvc;
using KasraLoan.Application.Features.Authentication.Login;
using KasraLoan.Application.DTOs.Auth;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _mediator.Send(new LoginCommand
            {
                LoginRequest = request
            });

            return Ok(result);
        }
    }
}