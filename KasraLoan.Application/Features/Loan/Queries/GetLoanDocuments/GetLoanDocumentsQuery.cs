using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Queries.GetLoanDocuments
{
    public class GetLoanDocumentsQuery : IRequest<List<GetLoanDocumentsResponse>>
    {
        public Guid LoanRequestId { get; set; }
    }
}