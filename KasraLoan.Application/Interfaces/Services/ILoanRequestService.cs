using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface ILoanRequestService
    {
        Task<ApiResponse<Guid>> CreateLoanRequestAsync(string employeeId,CreateLoanRequestDto dto);
        Task<ApiResponse<bool>> RejectLoanAsync(Guid LoanId);
        Task<ApiResponse<bool>> ApproveLoanAsync(Guid LoanId);
    }
}