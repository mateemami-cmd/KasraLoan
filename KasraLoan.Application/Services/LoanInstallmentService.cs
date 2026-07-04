using KasraLoan.Application.Common.Results;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    public class LoanInstallmentService : ILoanInstallmentService
    {
        private readonly ILoanInstallmentRepository _repo;

        public LoanInstallmentService(ILoanInstallmentRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<List<LoanInstallmentDto>>> GetLoanInstallmentsAsync(Guid loanId)
        {
            var installments = await _repo.GetByLoanIdAsync(loanId);

            var result = installments.Select(x => new LoanInstallmentDto
            {
                InstallmentNumber = x.InstallmentNumber,
                Amount = x.Amount,
                DueDate = x.DueDate,
                IsPaid = x.IsPaid
            }).ToList();

            return new ApiResponse<List<LoanInstallmentDto>>
            {
                IsSuccess = true,
                Data = result
            };
        }
    }
}