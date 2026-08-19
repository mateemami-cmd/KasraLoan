using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface ILoanPermissionRequestRepository
    {
        Task AddAsync(LoanPermissionRequest request);

        Task<LoanPermissionRequest?> GetByIdAsync(Guid id);

        Task<List<LoanPermissionRequest>> GetByEmployeeIdAsync(Guid employeeId);

        /// <summary>
        /// آیا کارمند همین حالا یک درخواست مجوز در وضعیت Pending دارد؟
        /// برای جلوگیری از ثبت درخواست‌های تکراری استفاده می‌شود.
        /// </summary>
        Task<bool> HasPendingRequestAsync(Guid employeeId);

        // loanTypeId اختیاری: برای «ادمین وام» فقط درخواست‌های مجوزِ همان نوع وام برگردد.
        Task<List<LoanPermissionRequest>> GetPagedAsync(int page, int pageSize, LoanPermissionRequestStatus? status, int? loanTypeId = null);

        Task<int> GetPagedCountAsync(LoanPermissionRequestStatus? status, int? loanTypeId = null);

        Task SaveChangesAsync();
    }
}
