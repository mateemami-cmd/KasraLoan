using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Enums;
using KasraLoan.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.RejectLoan
{
    public class RejectLoanHandler : IRequestHandler<RejectLoanCommand, RejectLoanResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IAuditLogService _auditLogService;

        public RejectLoanHandler(ILoanRequestRepository loanRequestRepository, IAuditLogService auditLogService)
        {
            _loanRequestRepository = loanRequestRepository;
            _auditLogService = auditLogService;
        }

        public async Task<RejectLoanResponse> Handle(RejectLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            if (loan.Status != LoanStatus.Pending)
                throw new Exception("این وام قابل رد نیست");

            loan.Status = LoanStatus.Rejected;

            await _loanRequestRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(loan.EmployeeId, loan.Id, "RejectLoan", "Loan rejected by admin.");

            return new RejectLoanResponse
            {
                Message = "وام رد شد"
            };
        }
    }
}