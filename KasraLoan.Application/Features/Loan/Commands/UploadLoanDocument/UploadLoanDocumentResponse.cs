using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.UploadLoanDocument
{
    public class UploadLoanDocumentResponse
    {
        public string Message { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }
    }
}