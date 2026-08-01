using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoanRequestRepository
    {
        Task AddAsync(LoanRequest request);

        Task<LoanRequest?> GetByIdAsync(Guid id);

        Task<List<LoanRequest>> GetByEmployeeIdAsync(Guid employeeId);

        Task<LoanRequest?> GetPendingLoanByEmployeeIdAsync(Guid employeeId);

        /// <summary>
        /// آیا کارمند در حال حاضر وامی دارد که هنوز تسویه یا رد نشده است
        /// (وضعیت Pending، Approved یا Active)؟ اگر بله، اجازه‌ی ثبت درخواست جدید ندارد.
        /// </summary>
        Task<bool> HasActiveLoanAsync(Guid employeeId);

        Task<List<LoanRequest>> GetAllAsync();

        Task<int> GetTotalCountAsync();

        Task<int> GetPendingCountAsync();

        Task<int> GetApprovedCountAsync();

        Task<int> GetRejectedCountAsync();

        Task<decimal> GetTotalRequestedAmountAsync();

        Task<decimal> GetTotalApprovedAmountAsync();

        Task<List<LoanRequest>> GetPagedAsync(int page, int pageSize, LoanStatus? status, string? search);

        /// <summary>
        /// تعداد کل رکوردهایی که با همین فیلترهای GetPagedAsync مطابقت دارند
        /// (برای محاسبه‌ی تعداد کل صفحات، مستقل از page/pageSize).
        /// </summary>
        Task<int> GetPagedCountAsync(LoanStatus? status, string? search);

        Task SaveChangesAsync();
    }
}