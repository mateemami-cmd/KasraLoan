using KasraLoan.Application.Common.Results;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using KasraLoan.Domain.Services;

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

            var existingPendingLoan = await _loanRequestRepository
            .GetPendingLoanByEmployeeIdAsync(employeeGuid);

            if (existingPendingLoan != null)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "شما یک درخواست وام در حال بررسی دارید"
                };
            }

            var loanType = await _loanTypeRepository.GetByIdAsync(dto.LoanTypeId);
            if (loanType == null)
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "نوع وام نامعتبر است"
                };

            if (!loanType.IsActive)
            {
                return new ApiResponse<Guid>
                {
                    IsSuccess = false,
                    Message = "این نوع وام در حال حاضر فعال نمی‌باشد."
                };
            }

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
                Status = LoanStatus.Pending,
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

        public async Task<ApiResponse<bool>> ApproveLoanAsync(Guid loanId)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(loanId);

            if (loan == null)
                return new ApiResponse<bool> { IsSuccess = false, Message = "وام یافت نشد" };

            if (loan.Status != LoanStatus.Pending)
                return new ApiResponse<bool> { IsSuccess = false, Message = "این وام قابل تأیید نیست" };

            loan.Status = LoanStatus.Approved;

            //await _loanRequestRepository.UpdateAsync(loan);
            await _loanRequestRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                Message = "وام تأیید شد",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> RejectLoanAsync(Guid loanId)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(loanId);

            if (loan == null)
                return new ApiResponse<bool> { IsSuccess = false, Message = "وام یافت نشد" };

            if (loan.Status != LoanStatus.Pending)
                return new ApiResponse<bool> { IsSuccess = false, Message = "این وام قابل رد نیست" };

            loan.Status = LoanStatus.Rejected;

            //await _loanRequestRepository.UpdateAsync(loan);
            await _loanRequestRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                Message = "وام رد شد",
                Data = true
            };
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
            }).ToList();

            return new ApiResponse<List<LoanRequestDto>>
            {
                IsSuccess = true,
                Data = result
            };
        }
    }
}