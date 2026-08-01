using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;

namespace KasraLoan.Application.Services
{
    public class LoanRequestService : ILoanRequestService
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly ILoanCalculationService _loanCalculationService;
        private readonly ILoanRuleEngine _loanRuleEngine;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;

        public LoanRequestService(
        ILoanRequestRepository loanRequestRepository,
        IEmployeeRepository employeeRepository,
        ILoanTypeRepository loanTypeRepository,
        ILoanCalculationService loanCalculationService,
        ILoanRuleEngine loanRuleEngine,
        IEmployeeScoreRepository employeeScoreRepository,
        ILoanInstallmentRepository loanInstallmentRepository)
        {
            _loanRequestRepository = loanRequestRepository;
            _employeeRepository = employeeRepository;
            _loanTypeRepository = loanTypeRepository;
            _loanCalculationService = loanCalculationService;
            _loanRuleEngine = loanRuleEngine;
            _employeeScoreRepository = employeeScoreRepository;
            _loanInstallmentRepository = loanInstallmentRepository;
        }
        
        public async Task<ApiResponse<List<LoanRequestDto>>> GetLoansByEmployeeIdAsync(Guid employeeId)
        {
            var loans = await _loanRequestRepository.GetByEmployeeIdAsync(employeeId);

            var result = loans.Select(x => new LoanRequestDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                LoanTypeId = x.LoanTypeId,
                RequestedAmount = x.RequestedAmount,
                ApprovedAmount = x.ApprovedAmount,
                InstallmentCount = x.InstallmentCount,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                TotalPayableAmount = x.TotalPayableAmount,
                MonthlyPaymentAmount = x.MonthlyPaymentAmount
            })
                .ToList();

            return new ApiResponse<List<LoanRequestDto>>
            {
                IsSuccess = true,
                Data = result
            };
        }

        public async Task<ApiResponse<List<LoanRequestDto>>> GetAllLoansAsync()
        {
            var loans = await _loanRequestRepository.GetAllAsync();

            var result = loans.Select(x => new LoanRequestDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                LoanTypeId = x.LoanTypeId,
                RequestedAmount = x.RequestedAmount,
                ApprovedAmount = x.ApprovedAmount,
                InstallmentCount = x.InstallmentCount,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                TotalPayableAmount = x.TotalPayableAmount,
                MonthlyPaymentAmount = x.MonthlyPaymentAmount
            })
                .ToList();

            return new ApiResponse<List<LoanRequestDto>>
            {
                IsSuccess = true,
                Data = result
            };
        }

        public async Task<ApiResponse<List<LoanRequestDto>>> GetAdminLoansAsync(LoanStatus? status)
        {
            var loans = await _loanRequestRepository.GetAllAsync();

            if (status.HasValue)
            {
                loans = loans.Where(x => x.Status == status.Value).ToList();
            }

            var result = loans.Select(x => new LoanRequestDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                LoanTypeId = x.LoanTypeId,
                RequestedAmount = x.RequestedAmount,
                ApprovedAmount = x.ApprovedAmount,
                InstallmentCount = x.InstallmentCount,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                TotalPayableAmount = x.TotalPayableAmount,
                MonthlyPaymentAmount = x.MonthlyPaymentAmount
            })
                .ToList();

            return new ApiResponse<List<LoanRequestDto>>
            {
                IsSuccess = true,
                Data = result
            };
        }
    }
}