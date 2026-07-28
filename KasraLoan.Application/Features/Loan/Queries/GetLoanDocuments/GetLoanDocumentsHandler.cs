using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanDocuments
{
    public class GetLoanDocumentsHandler : IRequestHandler<GetLoanDocumentsQuery, List<GetLoanDocumentsResponse>>
    {
        private readonly ILoanDocumentRepository _loanDocumentRepository;
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLoanDocumentsHandler(ILoanDocumentRepository loanDocumentRepository, ILoanRequestRepository loanRequestRepository, ICurrentUserService currentUserService)
        {
            _loanDocumentRepository = loanDocumentRepository;
            _loanRequestRepository = loanRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<GetLoanDocumentsResponse>> Handle(GetLoanDocumentsQuery request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            var isAdmin = string.Equals(_currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && loan.EmployeeId != _currentUserService.UserId)
                throw new ForbiddenAccessException("شما اجازه‌ی مشاهده‌ی مدارک این وام را ندارید.");

            var documents = await _loanDocumentRepository.GetByLoanIdAsync(request.LoanRequestId);

            return documents.Select(x => new GetLoanDocumentsResponse
            {
                Id = x.Id,
                FileName = x.FileName,
                FilePath = x.FilePath,
                UploadedAt = x.UploadedAt
            })
                .ToList();
        }
    }
}