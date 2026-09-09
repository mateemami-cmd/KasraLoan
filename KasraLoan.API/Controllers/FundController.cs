using System.Threading.Tasks;
using KasraLoan.API.Authorization;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FundController : ControllerBase
    {
        private readonly ILoanFundRepository _loanFundRepository;
        private readonly IFundContributionService _fundContributionService;

        public FundController(
            ILoanFundRepository loanFundRepository,
            IFundContributionService fundContributionService)
        {
            _loanFundRepository = loanFundRepository;
            _fundContributionService = fundContributionService;
        }

        /// <summary>موجودیِ فعلیِ صندوق و آخرین ماهِ واریز.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            var fund = await _loanFundRepository.GetAsync();

            return Ok(new
            {
                balance = fund?.Balance ?? 0,
                lastContributionPeriod = fund?.LastContributionPeriod
            });
        }

        /// <summary>
        /// اجرای دستیِ واریزِ ماهانه (۳٪ حقوقِ کارمندان به صندوق). این کار به‌صورت
        /// خودکار اولِ هر ماهِ شمسی انجام می‌شود؛ این endpoint برای اجرای فوری/دمو است.
        /// </summary>
        [HttpPost("contribute")]
        [Authorize(Policy = LoanPolicies.SeniorAdminOnly)]
        public async Task<IActionResult> Contribute()
        {
            var result = await _fundContributionService.ApplyMonthlyContributionAsync(force: true);

            return Ok(result);
        }
    }
}
