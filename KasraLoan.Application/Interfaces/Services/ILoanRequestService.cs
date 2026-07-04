using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface ILoanRequestService
    {
        Task<ApiResponse<Guid>> CreateLoanRequestAsync(string employeeId, CreateLoanRequestDto dto);
        Task<ApiResponse<bool>> RejectLoanAsync(Guid LoanId);
        Task<ApiResponse<bool>> ApproveLoanAsync(Guid LoanId);
        Task<ApiResponse<List<LoanRequestDto>>> GetLoansByEmployeeIdAsync(Guid employeeId);
        Task<ApiResponse<List<LoanRequestDto>>> GetAllLoansAsync();
        Task<ApiResponse<List<LoanRequestDto>>> GetAdminLoansAsync(LoanStatus? status);
    }
}


        //Task<ApiResponse<List<LoanRequestDto>>> GetPendingLoanByEmployeeIdAsync(Guid employeeId);