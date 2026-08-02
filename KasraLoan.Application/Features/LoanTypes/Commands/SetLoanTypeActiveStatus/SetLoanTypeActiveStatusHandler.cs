using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanTypes.Commands.SetLoanTypeActiveStatus
{
    public class SetLoanTypeActiveStatusHandler
        : IRequestHandler<SetLoanTypeActiveStatusCommand, SetLoanTypeActiveStatusResponse>
    {
        private readonly ILoanTypeRepository _loanTypeRepository;

        public SetLoanTypeActiveStatusHandler(ILoanTypeRepository loanTypeRepository)
        {
            _loanTypeRepository = loanTypeRepository;
        }

        public async Task<SetLoanTypeActiveStatusResponse> Handle(
            SetLoanTypeActiveStatusCommand request,
            CancellationToken cancellationToken)
        {
            var loanType = await _loanTypeRepository.GetByIdAsync(request.LoanTypeId);

            if (loanType == null)
                throw new KeyNotFoundException("Loan type not found");

            loanType.IsActive = request.IsActive;

            await _loanTypeRepository.SaveChangesAsync();

            return new SetLoanTypeActiveStatusResponse
            {
                LoanTypeId = loanType.Id,
                IsActive = loanType.IsActive,
                Message = loanType.IsActive
                    ? $"وام «{loanType.Name}» فعال شد."
                    : $"وام «{loanType.Name}» غیرفعال شد."
            };
        }
    }
}
