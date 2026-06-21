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
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;

        public LoanRequestService(
        ILoanRequestRepository loanRequestRepository,
        IEmployeeRepository employeeRepository,
        ILoanTypeRepository loanTypeRepository,
        ILoanCalculationService loanCalculationService,
        LoanRuleEngine loanRuleEngine,
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

        public async Task<ApiResponse<Guid>> CreateLoanRequestAsync(string employeeId, CreateLoanRequestDto dto)
        {
            if (dto.RequestedAmount <= 0)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "مبلغ وام نامعتبر است"
                };
            }

            if (dto.InstallmentCount <= 0)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "تعداد اقساط نامعتبر است"
                };
            }

            var employeeGuid = Guid.Parse(employeeId);
            var employee = await _employeeRepository.GetByIdAsync(employeeGuid);
            if (employee == null)
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "کاربر یافت نشد"
                };

            var loanType = await _loanTypeRepository.GetByIdAsync(dto.LoanTypeId);
            if (loanType == null)
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "نوع وام نامعتبر است"
                };

            var score = await _employeeScoreRepository.GetScoreByEmployeeIdAsync(employeeGuid);

            if (score < 600)
                return new ApiResponse<Guid>
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
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = ruleResult.Message ?? "درخواست رد شد"
                };
            }

            var baseInstallment = dto.RequestedAmount / dto.InstallmentCount;
            var loan = new LoanRequest
            {
                EmployeeId = employeeGuid,
                LoanTypeId = dto.LoanTypeId,
                RequestedAmount = dto.RequestedAmount,
                ApprovedAmount = dto.RequestedAmount,
                InstallmentCount = dto.InstallmentCount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                TotalPayableAmount = dto.RequestedAmount,

                MonthlyPaymentAmount =
                baseInstallment +
                ((dto.RequestedAmount * ruleResult.MonthlyFeePercent) / 100)
            };

            await _loanRequestRepository.AddAsync(loan);
            await _loanRequestRepository.SaveChangesAsync();

            var installments = new List<LoanInstallment>();

            for (int i = 1; i <= loan.InstallmentCount; i++)
            {
                installments.Add(new LoanInstallment
                {
                    LoanRequestId = loan.Id,
                    InstallmentNumber = i,
                    Amount = loan.MonthlyPaymentAmount,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    IsPaid = false
                });
            }

            await _loanInstallmentRepository.AddRangeAsync(installments);
            await _loanInstallmentRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                IsSuccess = true,
                Message = "درخواست با موفقیت ثبت شد",
                Data = loan.Id
            };
        }
    }
}