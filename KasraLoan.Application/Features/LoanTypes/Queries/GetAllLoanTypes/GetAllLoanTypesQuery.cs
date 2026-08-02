using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanTypes.Queries.GetAllLoanTypes
{
    public class GetAllLoanTypesQuery : IRequest<GetAllLoanTypesResponse>
    {
        /// <summary>اگر true باشد فقط وام‌های فعال برمی‌گردند.</summary>
        public bool ActiveOnly { get; set; }
    }
}
