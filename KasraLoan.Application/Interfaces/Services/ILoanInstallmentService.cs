using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface ILoanInstallmentService
    {
        Task<ApiResponse<List<LoanInstallmentDto>>> GetLoanInstallmentsAsync(Guid loanId);
        Task<ApiResponse<bool>> PayInstallmentAsync(Guid installmentId, Guid employeeId);
    }
}