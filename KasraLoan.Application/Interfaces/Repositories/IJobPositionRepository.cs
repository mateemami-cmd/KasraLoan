using KasraLoan.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IJobPositionRepository
    {
        Task<JobPosition?> GetByIdAsync(int id);

        Task<List<JobPosition>> GetAllAsync(bool activeOnly);

        Task<bool> TitleExistsAsync(string title, int? excludeId = null);

        Task<bool> HasEmployeesAsync(int jobPositionId);

        /// <summary>تعداد کارمندان هر سمت، کلیدشده با شناسه‌ی سمت.</summary>
        Task<Dictionary<int, int>> GetEmployeeCountsAsync();

        Task AddAsync(JobPosition jobPosition);

        Task SaveChangesAsync();
    }
}
