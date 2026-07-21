using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Entities;
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
        private readonly ILoanRequestRepository _loanRequestRepository;

        public LoanInstallmentService(ILoanInstallmentRepository repo, ILoanRequestRepository loanRequestRepository)
        {
            _repo = repo;
            _loanRequestRepository = loanRequestRepository;
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

        public async Task CreateInstallmentsAsync(Guid loanRequestId)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(loanRequestId);

            if (loan == null)
                throw new Exception("وام یافت نشد");

            if (loan.ApprovedAmount <= 0)
                throw new Exception("وام هنوز مبلغ تأیید شده ندارد");

            if (loan.InstallmentCount <= 0)
                throw new Exception("تعداد اقساط نامعتبر است");

            var installmentAmount =
                loan.TotalPayableAmount / loan.InstallmentCount;

            var installments = new List<LoanInstallment>();

            for (int i = 1; i <= loan.InstallmentCount; i++)
            {
                installments.Add(new LoanInstallment
                {
                    Id = Guid.NewGuid(),

                    LoanRequestId = loan.Id,

                    InstallmentNumber = i,

                    Amount = installmentAmount,

                    DueDate = DateTime.UtcNow.Date.AddMonths(i),

                    IsPaid = false,

                    CreatedAt = DateTime.UtcNow
                });
            }

            await _repo.AddRangeAsync(installments);

            await _repo.SaveChangesAsync();
        }
    }
}