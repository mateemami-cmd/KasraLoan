using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Features.Authentication.Login;
using KasraLoan.Application.Features.Authentication.Logout;
using KasraLoan.Application.Features.Authentication.Refresh;
using KasraLoan.Application.Features.Employee.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            await _mediator.Send(new LogoutCommand
            {
                RefreshToken = request.RefreshToken
            });

            return Ok(new
            {
                Message = "Logged out successfully."
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
           [FromBody] RefreshTokenRequestDto request)
        {
            var result = await _mediator.Send(new RefreshCommand
            {
                Request = request
            });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _mediator.Send(new GetCurrentUserQuery());

            return Ok(result);
        }
    }
}