using KasraLoan.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string token)
        {
            var jwt = await _authService.LoginByToken(token);

            if (jwt is null)
                return Unauthorized();

            return Ok(new { token = jwt });
        }
    }
}