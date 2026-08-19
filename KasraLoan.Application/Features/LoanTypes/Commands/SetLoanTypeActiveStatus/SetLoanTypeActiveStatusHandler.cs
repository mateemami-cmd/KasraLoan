using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
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
        private readonly ICurrentUserService _currentUserService;

        public SetLoanTypeActiveStatusHandler(
            ILoanTypeRepository loanTypeRepository,
            ICurrentUserService currentUserService)
        {
            _loanTypeRepository = loanTypeRepository;
            _currentUserService = currentUserService;
        }

        public async Task<SetLoanTypeActiveStatusResponse> Handle(
            SetLoanTypeActiveStatusCommand request,
            CancellationToken cancellationToken)
        {
            // ادمین وام فقط می‌تواند تنظیماتِ وامِ خودش را عوض کند.
            if (!_currentUserService.CanManageLoanType(request.LoanTypeId))
                throw new BusinessRuleException("شما به این نوع وام دسترسی ندارید.");

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
