using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.UploadLoanDocument
{
    public class UploadLoanDocumentCommand
        : IRequest<UploadLoanDocumentResponse>
    {
        public Guid LoanRequestId { get; set; }

        public byte[] FileContent { get; set; } = [];

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}