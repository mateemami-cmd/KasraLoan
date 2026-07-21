using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanDocuments
{
    public class GetLoanDocumentsHandler
    : IRequestHandler<GetLoanDocumentsQuery, List<GetLoanDocumentsResponse>>
    {
        private readonly ILoanDocumentRepository _loanDocumentRepository;

        public GetLoanDocumentsHandler(
            ILoanDocumentRepository loanDocumentRepository)
        {
            _loanDocumentRepository = loanDocumentRepository;
        }

        public async Task<List<GetLoanDocumentsResponse>> Handle(
            GetLoanDocumentsQuery request,
            CancellationToken cancellationToken)
        {
            var documents = await _loanDocumentRepository
                .GetByLoanIdAsync(request.LoanRequestId);

            return documents.Select(x => new GetLoanDocumentsResponse
            {
                Id = x.Id,
                FileName = x.FileName,
                FilePath = x.FilePath,
                UploadedAt = x.UploadedAt
            }).ToList();
        }
    }
}