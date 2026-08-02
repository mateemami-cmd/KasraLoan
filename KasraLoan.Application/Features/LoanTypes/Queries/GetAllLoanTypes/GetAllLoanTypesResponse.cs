using KasraLoan.Application.DTOs.LoanTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanTypes.Queries.GetAllLoanTypes
{
    public class GetAllLoanTypesResponse
    {
        public List<LoanTypeDto> Items { get; set; } = new();
    }
}
