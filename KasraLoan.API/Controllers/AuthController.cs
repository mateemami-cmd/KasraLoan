using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Features.Authentication.ChangePassword;
using KasraLoan.Application.Features.Authentication.ForgotPassword;
using KasraLoan.Application.Features.Authentication.GetLoginHistory;
using KasraLoan.Application.Features.Authentication.Login;
using KasraLoan.Application.Features.Authentication.Logout;
using KasraLoan.Application.Features.Authentication.Refresh;
using KasraLoan.Application.Features.Authentication.ResetPassword;
using KasraLoan.Application.Features.Authentication.Sessions;
using KasraLoan.Application.Features.Employee.Commands.DeleteProfilePicture;
using KasraLoan.Application.Features.Employee.Commands.UpdateProfile;
using KasraLoan.Application.Features.Employee.Commands.UploadProfilePicture;
using KasraLoan.Application.Features.Employee.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;

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
                IpAddress = GetClientIp()
            });

            return Ok(result);
        }

        /// <summary>
        /// آدرس IP واقعیِ کاربر. اگر از پشتِ پروکسی/دِو‌سرور آمده، از هدر
        /// X-Forwarded-For خوانده می‌شود؛ لوپ‌بکِ IPv6 (::1) به 127.0.0.1 و
        /// آدرس‌های IPv4-mapped به IPv4 ساده تبدیل می‌شوند تا خوانا باشند.
        /// </summary>
        private string? GetClientIp()
        {
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                var first = forwarded.Split(',')[0].Trim();
                if (IPAddress.TryParse(first, out var fip))
                    return Normalize(fip);
                return first;
            }

            var ip = HttpContext.Connection.RemoteIpAddress;
            return ip == null ? null : Normalize(ip);
        }

        private static string Normalize(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return "127.0.0.1";
            if (ip.IsIPv4MappedToIPv6) return ip.MapToIPv4().ToString();
            return ip.ToString();
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

        // فراموشیِ رمز عبور: کاربر ایمیلش را می‌دهد، رمزِ موقت به ایمیلش می‌رود.
        // بدونِ احراز هویت (چون کاربر نمی‌تواند وارد شود).
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var result = await _mediator.Send(new ForgotPasswordCommand { Request = request });

            return Ok(result);
        }

        // تعیینِ رمزِ جدید بعد از ورود با رمزِ موقت. کاربر واردشده است (Authorize)،
        // ولی رمزِ فعلی گرفته نمی‌شود چون موقت است و خودش نمی‌داند.
        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _mediator.Send(new ResetPasswordCommand { Request = request });

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _mediator.Send(new GetCurrentUserQuery());

            return Ok(result);
        }

        // تغییر رمز عبورِ کاربرِ جاری؛ رمز فعلی تأیید می‌شود.
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var result = await _mediator.Send(new ChangePasswordCommand { Request = request });

            return Ok(result);
        }

        // سه ورودِ اخیرِ کاربرِ جاری (برای «تاریخچه ورودهای اخیر»).
        [Authorize]
        [HttpGet("login-history")]
        public async Task<IActionResult> GetLoginHistory()
        {
            var result = await _mediator.Send(new GetLoginHistoryQuery());

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