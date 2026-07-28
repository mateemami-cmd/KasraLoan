using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestHandler : IRequestHandler<CreateLoanRequestCommand, CreateLoanRequestResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly ILoanRuleEngine _loanRuleEngine;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        public CreateLoanRequestHandler(ILoanRequestRepository loanRequestRepository, ILoanTypeRepository loanTypeRepository, IEmployeeRepository employeeRepository, IEmployeeScoreRepository employeeScoreRepository, ILoanRuleEngine loanRuleEngine, ICurrentUserService currentUserService, INotificationService notificationService)
        {
            _loanRequestRepository = loanRequestRepository;
            _loanTypeRepository = loanTypeRepository;
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _loanRuleEngine = loanRuleEngine;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<CreateLoanRequestResponse> Handle(CreateLoanRequestCommand request, CancellationToken cancellationToken)
        {

            var employeeId = _currentUserService.UserId;

            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found");


            var loanType = await _loanTypeRepository.GetByIdAsync(request.Request.LoanTypeId);

            if (loanType == null)
                throw new KeyNotFoundException("Loan type not found");


            var employeeScore = await _employeeScoreRepository.GetByEmployeeIdAsync(employeeId);


            if (employeeScore == null)
                throw new KeyNotFoundException("Employee score not found");


            var context = new LoanRuleContext
            {
                Employee = employee,
                LoanType = loanType,
                RequestedAmount = request.Request.RequestedAmount,
                EmployeeScore = employeeScore.Score
            };


            var ruleResult = _loanRuleEngine.Evaluate(context);


            if (!ruleResult.IsAllowed)
            {
                throw new Exception(ruleResult.Message);
            }

            var approvedAmount = Math.Min(
                request.Request.RequestedAmount,
                (int)ruleResult.MaxAllowedAmount);

            var installmentCount = Math.Min(
                request.Request.InstallmentCount,
                ruleResult.MaxInstallments);

            var loanRequest = new Domain.Entities.LoanRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                LoanTypeId = loanType.Id,
                RequestedAmount = request.Request.RequestedAmount,
                ApprovedAmount = approvedAmount,
                InstallmentCount = installmentCount,
                MonthlyFeePercent = ruleResult.MonthlyFeePercent,
                Status = Domain.Enums.LoanStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };


            await _loanRequestRepository.AddAsync(loanRequest);
            await _loanRequestRepository.SaveChangesAsync();

            await _notificationService.SendAsync(
                loanRequest.EmployeeId,
                "ثبت درخواست وام",
                "درخواست وام شما با موفقیت ثبت شد و در انتظار بررسی است.");

            return new CreateLoanRequestResponse
            {
                LoanRequestId = loanRequest.Id,
                Message = "درخواست وام با موفقیت ثبت شد"
            };
        }
    }
}