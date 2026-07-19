using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.ApproveLoan
{
    public class ApproveLoanHandler
    : IRequestHandler<ApproveLoanCommand, ApproveLoanResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;

        public ApproveLoanHandler(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }

        public async Task<ApproveLoanResponse> Handle(
            ApproveLoanCommand request,
            CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository
                .GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            if (loan.Status != LoanStatus.Pending)
                throw new Exception("این وام قابل تأیید نیست");

            loan.Status = LoanStatus.Approved;

            await _loanRequestRepository.SaveChangesAsync();

            return new ApproveLoanResponse
            {
                Message = "وام تأیید شد"
            };
        }
    }
}