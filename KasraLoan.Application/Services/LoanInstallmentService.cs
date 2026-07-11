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
                Id = x.Id,
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

        public async Task<ApiResponse<bool>> PayInstallmentAsync(Guid installmentId, Guid employeeId)
        {
            //var installment = await _repo.GetByIdAsync(installmentId);
            var installment = await _repo.GetByIdWithLoanAsync(installmentId);

            if (installment == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "قسط مورد نظر یافت نشد."
                };
            }
            if (installment.LoanRequest.EmployeeId != employeeId)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "شما اجازه پرداخت این قسط را ندارید."
                };
            }
            if (installment.IsPaid)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "این قسط قبلاً پرداخت شده است."
                };
            }
            installment.IsPaid = true;

            await _repo.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "قسط با موفقیت پرداخت شد."
            };
        }
    }
}