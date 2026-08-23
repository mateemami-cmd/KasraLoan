using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);

        Task<RefreshToken?> GetByTokenAsync(string token);

        Task<RefreshToken?> GetByIdAsync(int id);

        /// <summary>نشست‌های فعالِ یک کارمند: باطل‌نشده و منقضی‌نشده، جدیدترین اول.</summary>
        Task<List<RefreshToken>> GetActiveByEmployeeAsync(Guid employeeId);

        /// <summary>
        /// نشستِ فعالِ همین کارمند روی همین دستگاه (بر اساس DeviceId)، اگر باشد.
        /// برای یکتاسازی: ورودِ دوباره با همین دستگاه همین نشست را به‌روز می‌کند.
        /// </summary>
        Task<RefreshToken?> GetActiveByEmployeeAndDeviceAsync(Guid employeeId, string deviceId);

        Task UpdateAsync(RefreshToken token);
    }
}