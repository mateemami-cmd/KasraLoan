using System;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KasraLoan.API.BackgroundJobs
{
    /// <summary>
    /// اولِ هر ماهِ شمسی، ۳٪ از حقوقِ کارمندانِ فعال را به صندوق واریز می‌کند.
    /// هر چند ساعت یک‌بار بیدار می‌شود و اگر امروز روزِ اولِ ماهِ شمسی باشد و این ماه
    /// هنوز واریز نشده باشد، واریز را انجام می‌دهد (سرویس، تکراری‌بودن را خودش مهار می‌کند).
    /// </summary>
    public class MonthlyFundContributionWorker : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlyFundContributionWorker> _logger;

        public MonthlyFundContributionWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlyFundContributionWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var calendar = scope.ServiceProvider.GetRequiredService<IPayrollCalendarService>();

                    if (calendar.GetPersianDayOfMonth(DateTime.UtcNow) == 1)
                    {
                        var contribution = scope.ServiceProvider
                            .GetRequiredService<IFundContributionService>();

                        var result = await contribution.ApplyMonthlyContributionAsync(force: false);

                        if (result.Applied)
                            _logger.LogInformation(
                                "واریزِ ماهانه‌ی صندوق انجام شد ({Period}): {Amount} تومان از {Count} کارمند.",
                                result.Period, result.AmountAdded, result.EmployeeCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در واریزِ ماهانه‌ی صندوق.");
                }

                try
                {
                    await Task.Delay(CheckInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
