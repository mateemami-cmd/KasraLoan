using KasraLoan.Application.Common.Logging;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KasraLoan.API.Controllers
{
    /// <summary>
    /// پرداخت اقساط: انتخاب روش، چک، و درگاه.
    ///
    /// این کنترلر عمداً مستقیم سرویس را صدا می‌زند و از MediatR رد نمی‌شود،
    /// چون یکی از ورودی‌هایش اطلاعات کارت بانکی است و نباید حتی احتمالِ
    /// نوشته‌شدنش در پایپ‌لاین لاگ وجود داشته باشد.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InstallmentPaymentController : ControllerBase
    {
        private readonly IInstallmentPaymentService _service;
        private readonly ICurrentUserService _currentUser;

        public InstallmentPaymentController(
            IInstallmentPaymentService service,
            ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        // ───────── کارمند ─────────

        /// <summary>قسط بعدی و این‌که الان می‌شود روش پرداخت را انتخاب کرد یا نه.</summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            return Ok(await _service.GetCurrentInstallmentAsync(_currentUser.UserId));
        }

        /// <summary>انتخاب روش پرداخت برای یک قسط.</summary>
        [HttpPost("{installmentId:guid}/method")]
        public async Task<IActionResult> SelectMethod(
            Guid installmentId,
            [FromBody] SelectPaymentMethodRequestDto request)
        {
            if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
                return BadRequest(new { Message = "روش پرداخت معتبر نیست." });

            return Ok(await _service.SelectMethodAsync(installmentId, _currentUser.UserId, method));
        }

        /// <summary>ثبت چک همراه تصویر. بعد از این، چک به صف تأیید ادمین می‌رود.</summary>
        [HttpPost("{installmentId:guid}/cheque")]
        public async Task<IActionResult> SubmitCheque(
            Guid installmentId,
            [FromForm] SubmitChequeRequestDto info,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "تصویر چک الزامی است." });

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var result = await _service.SubmitChequeAsync(
                installmentId,
                _currentUser.UserId,
                info,
                stream.ToArray(),
                file.FileName,
                file.ContentType);

            return Ok(result);
        }

        /// <summary>باز کردن نشست پرداخت آنلاین؛ خروجی مسیر صفحه‌ی درگاه است.</summary>
        [HttpPost("{installmentId:guid}/gateway")]
        public async Task<IActionResult> StartGatewayPayment(Guid installmentId)
        {
            return Ok(await _service.StartGatewayPaymentAsync(installmentId, _currentUser.UserId));
        }

        /// <summary>اطلاعات نمایشی صفحه‌ی درگاه (مبلغ، شماره قسط، مهلت).</summary>
        [HttpGet("gateway/{authority:guid}")]
        public async Task<IActionResult> GetGatewaySession(Guid authority)
        {
            return Ok(await _service.GetGatewaySessionAsync(authority));
        }

        /// <summary>
        /// نهایی کردن پرداخت آنلاین.
        /// اطلاعات کارت فقط اعتبارسنجی می‌شوند و هیچ‌کجا ذخیره یا لاگ نمی‌شوند.
        /// </summary>
        [HttpPost("gateway/{authority:guid}/pay")]
        public async Task<IActionResult> CompleteGatewayPayment(
            Guid authority,
            [FromBody] GatewayPaymentRequestDto card)
        {
            return Ok(await _service.CompleteGatewayPaymentAsync(authority, card));
        }

        // ───────── ادمین ─────────

        /// <summary>صف چک‌های منتظر تأیید، قدیمی‌ترین اول.</summary>
        [HttpGet("cheques/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingCheques()
        {
            return Ok(await _service.GetPendingChequesAsync());
        }

        [HttpPost("cheques/{paymentId:guid}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmCheque(Guid paymentId)
        {
            return Ok(await _service.ConfirmChequeAsync(paymentId, _currentUser.UserId));
        }

        [HttpPost("cheques/{paymentId:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectCheque(
            Guid paymentId,
            [FromBody] RejectLoanRequestDto body)
        {
            return Ok(await _service.RejectChequeAsync(
                paymentId, _currentUser.UserId, body?.RejectReason ?? string.Empty));
        }
    }
}
