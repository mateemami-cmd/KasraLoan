using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Enums;
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

        public RejectLoanHandler(ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<RejectLoanResponse> Handle(
            RejectLoanCommand request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            if (loan.Status != LoanStatus.Pending)
                throw new Exception("این وام قابل رد نیست");

            loan.Status = LoanStatus.Rejected;

            await _loanRequestRepository.SaveChangesAsync();

            return new RejectLoanResponse
            {
                Message = "وام رد شد"
            };
        }
    }
}