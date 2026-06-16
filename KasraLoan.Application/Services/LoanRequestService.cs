using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Services
{
    public class LoanRequestService : ILoanRequestService
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly ILoanCalculationService _loanCalculationService;
        private readonly LoanRuleEngine _loanRuleEngine;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;

        public LoanRequestService(
        ILoanRequestRepository loanRequestRepository,
        IEmployeeRepository employeeRepository,
        ILoanTypeRepository loanTypeRepository,
        ILoanCalculationService loanCalculationService,
        LoanRuleEngine loanRuleEngine,
        IEmployeeScoreRepository employeeScoreRepository)
        {
            _loanRequestRepository = loanRequestRepository;
            _employeeRepository = employeeRepository;
            _loanTypeRepository = loanTypeRepository;
            _loanCalculationService = loanCalculationService;
            _loanRuleEngine = loanRuleEngine;
            _employeeScoreRepository = employeeScoreRepository;
        }

        public async Task<ApiResponse<int?>> CreateLoanRequestAsync(CreateLoanRequestDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
                return new ApiResponse<int?>
                {
                    IsSuccess = false,
                    Message = "کاربر یافت نشد"
                };

            var loanType = await _loanTypeRepository.GetByIdAsync(dto.LoanTypeId);
            if (loanType == null)
                return new ApiResponse<int?>
                {
                    IsSuccess = false,
                    Message = "نوع وام نامعتبر است"
                };

            var score = await _employeeScoreRepository.GetScoreByEmployeeIdAsync(dto.EmployeeId);

            if (score < 600)
                return new ApiResponse<int?>
                {
                    IsSuccess = false,
                    Message = "امتیاز کاربر کافی نیست"
                };

            var context = new LoanRuleContext
            {
                Employee = employee,
                LoanType = loanType,
                RequestedAmount = dto.RequestedAmount,
                EmployeeScore = score
            };

            var ruleResult = _loanRuleEngine.Evaluate(context);

            if (!ruleResult.IsAllowed)
            {
                return new ApiResponse<int?>
                {
                    IsSuccess = false,
                    Message = ruleResult.Message ?? "درخواست رد شد"
                };
            }

            var loan = new LoanRequest
            {
                EmployeeId = dto.EmployeeId,
                LoanTypeId = dto.LoanTypeId,
                RequestedAmount = dto.RequestedAmount,
                ApprovedAmount = dto.RequestedAmount,
                InstallmentCount = dto.InstallmentCount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                TotalPayableAmount = dto.RequestedAmount,

                MonthlyPaymentAmount =
                    (dto.RequestedAmount / dto.InstallmentCount) +
                    ((dto.RequestedAmount * ruleResult.MonthlyFeePercent) / 100)
            };

            await _loanRequestRepository.AddAsync(loan);
            await _loanRequestRepository.SaveChangesAsync();

            return new ApiResponse<int?>
            {
                IsSuccess = true,
                Message = "درخواست با موفقیت ثبت شد",
                Data = loan.Id
            };
        }
    }
}