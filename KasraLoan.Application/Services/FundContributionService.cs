using System;
using System.Linq;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Enums;

namespace KasraLoan.Application.Services
{
    /// <inheritdoc cref="IFundContributionService"/>
    public class FundContributionService : IFundContributionService
    {
        // سهمِ ماهانه: ۳٪ از حقوقِ هر کارمند.
        private const decimal ContributionRate = 0.03m;

        private readonly ILoanFundRepository _loanFundRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeSalaryService _employeeSalaryService;
        private readonly IPayrollCalendarService _payrollCalendarService;

        public FundContributionService(
            ILoanFundRepository loanFundRepository,
            IEmployeeRepository employeeRepository,
            IEmployeeSalaryService employeeSalaryService,
            IPayrollCalendarService payrollCalendarService)
        {
            _loanFundRepository = loanFundRepository;
            _employeeRepository = employeeRepository;
            _employeeSalaryService = employeeSalaryService;
            _payrollCalendarService = payrollCalendarService;
        }

        public async Task<FundContributionResult> ApplyMonthlyContributionAsync(bool force = false)
        {
            var fund = await _loanFundRepository.GetAsync();
            if (fund == null)
                throw new InvalidOperationException("صندوقِ وام مقداردهی نشده است.");

            // ماهِ شمسیِ جاری به وقت ایران، به شکل «yyyy/MM».
            var period = _payrollCalendarService.ToPersianDateString(DateTime.UtcNow).Substring(0, 7);

            if (!force && fund.LastContributionPeriod == period)
            {
                return new FundContributionResult
                {
                    Applied = false,
                    Period = period,
                    NewBalance = fund.Balance,
                    Message = "واریزِ این ماه قبلاً انجام شده است."
                };
            }

            // فقط کارمندانِ فعال (نه ادمین، نه غیرفعال، نه حذف‌شده، نه پایان‌همکاری).
            var employees = (await _employeeRepository.GetAllAsync())
                .Where(e => e.Role == UserRole.Employee
                    && e.IsActive
                    && !e.IsDeleted
                    && e.EmploymentStatus == EmploymentStatus.Active)
                .ToList();

            long total = 0;
            foreach (var employee in employees)
            {
                var salary = _employeeSalaryService.GetEffectiveMonthlySalary(employee);
                total += (long)Math.Round(salary * ContributionRate, MidpointRounding.AwayFromZero);
            }

            fund.Balance += total;
            fund.LastContributionPeriod = period;
            fund.UpdatedAt = DateTime.UtcNow;
            await _loanFundRepository.SaveChangesAsync();

            return new FundContributionResult
            {
                Applied = true,
                AmountAdded = total,
                EmployeeCount = employees.Count,
                Period = period,
                NewBalance = fund.Balance,
                Message = $"واریزِ ماهانه انجام شد: {total:N0} تومان از {employees.Count} کارمند."
            };
        }
    }
}
