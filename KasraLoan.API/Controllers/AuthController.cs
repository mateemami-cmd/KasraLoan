using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Features.Authentication.Login;
using KasraLoan.Application.Features.Authentication.Logout;
using KasraLoan.Application.Features.Authentication.Refresh;
using KasraLoan.Application.Features.Authentication.Sessions;
using KasraLoan.Application.Features.Employee.Commands.DeleteProfilePicture;
using KasraLoan.Application.Features.Employee.Commands.UpdateProfile;
using KasraLoan.Application.Features.Employee.Commands.UploadProfilePicture;
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
                LoginRequest = request,
                UserAgent = Request.Headers.UserAgent.ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
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

        // نشست‌های فعالِ کاربرِ جاری (برای صفحه‌ی «نشست‌های فعال»).
        [Authorize]
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var result = await _mediator.Send(new GetActiveSessionsQuery());

            return Ok(result);
        }

        // قطعِ یکی از نشست‌های کاربرِ جاری (از راه دور خروج).
        [Authorize]
        [HttpPost("sessions/{sessionId:int}/revoke")]
        public async Task<IActionResult> RevokeSession(int sessionId)
        {
            var result = await _mediator.Send(new RevokeSessionCommand { SessionId = sessionId });

            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            var result = await _mediator.Send(new UpdateProfileCommand
            {
                Request = request
            });

            return Ok(result);
        }

        [Authorize]
        [HttpPost("profile/picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایلی انتخاب نشده است.");

            using var memoryStream = new MemoryStream();

            await file.CopyToAsync(memoryStream);

            var result = await _mediator.Send(new UploadProfilePictureCommand
            {
                FileContent = memoryStream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType
            });

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("profile/picture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var result = await _mediator.Send(new DeleteProfilePictureCommand());

            return Ok(result);
        }
    }
}