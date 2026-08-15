using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanQuote
{
    /// <summary>
    /// همان مسیری که ثبت درخواست طی می‌کند را بدون ذخیره‌سازی اجرا می‌کند، تا
    /// فرم دقیقاً همان اعدادی را نشان دهد که موقع ثبت اعمال می‌شوند.
    ///
    /// نکته‌ی مهم: هیچ فرمولی اینجا بازنویسی نشده — سقف از همان rule engine و
    /// مبالغ از همان LoanCalculationService می‌آیند.
    /// </summary>
    public class GetLoanQuoteHandler : IRequestHandler<GetLoanQuoteQuery, LoanQuoteDto>
    {
        /// <summary>کمینه مبلغ قابل درخواست.</summary>
        private const long MinimumAmount = 5_000_000;

        /// <summary>فاصله‌ی گزینه‌های مبلغ در لیست کشویی.</summary>
        private const long AmountStep = 5_000_000;

        /// <summary>
        /// پله‌های پیشنهادی تعداد اقساط. با سقف هر نوع وام فیلتر می‌شوند و خودِ
        /// سقف همیشه اضافه می‌شود — وگرنه وام سفر که سقفش ۱۰ قسط است فقط گزینه‌ی
        /// ۶ قسط می‌گرفت و کارمند اصلاً نمی‌توانست از حداکثر مدت استفاده کند.
        /// </summary>
        private static readonly int[] InstallmentLadder = { 3, 6, 9, 12, 18, 24, 36 };

        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly IEmployeeScoreService _employeeScoreService;
        private readonly IEmployeeSalaryService _employeeSalaryService;
        private readonly ILoanCalculationService _loanCalculationService;
        private readonly ILoanRuleEngine _loanRuleEngine;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPayrollCalendarService _payrollCalendar;

        public GetLoanQuoteHandler(
            ILoanTypeRepository loanTypeRepository,
            IEmployeeRepository employeeRepository,
            IEmployeeScoreRepository employeeScoreRepository,
            IEmployeeScoreService employeeScoreService,
            IEmployeeSalaryService employeeSalaryService,
            ILoanCalculationService loanCalculationService,
            ILoanRuleEngine loanRuleEngine,
            ICurrentUserService currentUserService,
            IPayrollCalendarService payrollCalendar)
        {
            _loanTypeRepository = loanTypeRepository;
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _employeeScoreService = employeeScoreService;
            _employeeSalaryService = employeeSalaryService;
            _loanCalculationService = loanCalculationService;
            _loanRuleEngine = loanRuleEngine;
            _currentUserService = currentUserService;
            _payrollCalendar = payrollCalendar;
        }

        public async Task<LoanQuoteDto> Handle(
            GetLoanQuoteQuery request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            var loanType = await _loanTypeRepository.GetByIdAsync(request.LoanTypeId);

            if (loanType == null)
                throw new KeyNotFoundException("نوع وام یافت نشد.");

            var quote = new LoanQuoteDto
            {
                LoanTypeId = loanType.Id,
                LoanTypeName = loanType.Name,
                MinAmount = MinimumAmount,
                AmountStep = AmountStep,
                MaxMonthlyInstallment = _employeeSalaryService.GetMaxMonthlyInstallment(employee),
                MarriageDate = employee.MarriageDate,
                MarriageDatePersian = employee.MarriageDate.HasValue
                    ? _payrollCalendar.ToPersianDateString(employee.MarriageDate.Value)
                    : null
            };

            if (!loanType.IsActive)
            {
                quote.IneligibilityReason = "این نوع وام در حال حاضر غیرفعال است.";
                return quote;
            }

            var scoreRecord = await _employeeScoreRepository.GetByEmployeeIdAsync(employee.Id);

            var effectiveScore = _employeeScoreService.GetEffectiveScore(employee, scoreRecord);

            if (scoreRecord?.HasLoanPermissionOverride == true)
            {
                effectiveScore = Math.Max(
                    effectiveScore, _employeeScoreService.MinimumScoreRequiredForLoan);
            }

            // وام ازدواج به تاریخ عقد نیاز دارد، ولی اگر در پروفایل نباشد کارمند
            // آن را در همین فرم وارد می‌کند. پس برای گرفتن سقف، یک تاریخ موقتِ
            // درون‌حافظه‌ای می‌گذاریم تا قانون بتواند اجرا شود و فرم رندر شود؛
            // این تاریخ ذخیره نمی‌شود (این هندلر query است و SaveChanges ندارد).
            // اعتبارسنجی واقعیِ تاریخ، موقع ثبت درخواست انجام می‌شود.
            if (loanType.Type == Domain.Enums.LoanTypeEnum.MarriageLoan
                && employee.MarriageDate == null)
            {
                employee.MarriageDate = DateTime.UtcNow;
            }

            // برای گرفتن سقف، قانون را با مبلغ ۱ صدا می‌زنیم: می‌خواهیم بدانیم
            // سقف چقدر است، نه این‌که مبلغ خاصی مجاز هست یا نه.
            var ceilingResult = _loanRuleEngine.Evaluate(new LoanRuleContext
            {
                Employee = employee,
                LoanType = loanType,
                RequestedAmount = 1,
                EmployeeScore = effectiveScore,
                RequestedInstallmentCount = InstallmentLadder.Max()
            });

            quote.AnnualFeePercent = ceilingResult.AnnualFeePercent;
            quote.RequiresDocument = ceilingResult.RequiresDocument;
            quote.RequiredDocumentDescription = ceilingResult.RequiredDocumentDescription;

            if (!ceilingResult.IsAllowed)
            {
                quote.IneligibilityReason = ceilingResult.Message;
                return quote;
            }

            quote.IsEligible = true;
            quote.MaxAmount = (long)ceilingResult.MaxAllowedAmount;

            if (quote.MaxAmount < MinimumAmount)
            {
                quote.IsEligible = false;
                quote.IneligibilityReason =
                    $"سقف وام شما ({quote.MaxAmount:N0} تومان) از کمینه مبلغ قابل درخواست " +
                    $"({MinimumAmount:N0} تومان) کمتر است.";
                return quote;
            }

            quote.AmountOptions = BuildAmountOptions(quote.MaxAmount);

            if (request.Amount is > 0)
            {
                quote.InstallmentOptions = BuildInstallmentOptions(
                    request.Amount.Value,
                    ceilingResult.AnnualFeePercent,
                    ceilingResult.MaxInstallments,
                    quote.MaxMonthlyInstallment);
            }

            return quote;
        }

        private static List<long> BuildAmountOptions(long maxAmount)
        {
            var options = new List<long>();

            for (var amount = MinimumAmount; amount <= maxAmount; amount += AmountStep)
                options.Add(amount);

            // اگر سقف مضرب دقیقِ گام نباشد، خودِ سقف هم به‌عنوان آخرین گزینه می‌آید
            // تا کارمند بتواند حداکثر مبلغ ممکن را انتخاب کند.
            if (options.Count == 0 || options[^1] != maxAmount)
                options.Add(maxAmount);

            return options;
        }

        private List<InstallmentOptionDto> BuildInstallmentOptions(
            long amount,
            decimal annualFeePercent,
            int maxInstallments,
            decimal maxMonthlyInstallment)
        {
            var counts = InstallmentLadder
                .Where(count => count <= maxInstallments)
                .ToList();

            if (maxInstallments > 0 && !counts.Contains(maxInstallments))
                counts.Add(maxInstallments);

            return counts
                .OrderBy(count => count)
                .Select(count =>
                {
                    var totalPayable = _loanCalculationService.CalculateTotalPayable(
                        amount, annualFeePercent, count);

                    var monthly = _loanCalculationService.CalculateMonthlyPayment(
                        totalPayable, count);

                    return new InstallmentOptionDto
                    {
                        InstallmentCount = count,
                        MonthlyPayment = monthly,
                        TotalPayable = totalPayable,
                        TotalFee = totalPayable - amount,
                        IsAffordable = maxMonthlyInstallment <= 0 || monthly <= maxMonthlyInstallment
                    };
                })
                .ToList();
        }
    }
}
