using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoginHistoryRepository
    {
        /// <summary>یک تلاشِ ورود را ثبت و بلافاصله ذخیره می‌کند (حتی اگر ورود ناموفق باشد).</summary>
        Task AddAsync(LoginHistory entry);

        /// <summary>آخرین n تلاشِ ورودِ یک کارمند، جدیدترین اول.</summary>
        Task<List<LoginHistory>> GetRecentByEmployeeAsync(Guid employeeId, int count);
    }
}
