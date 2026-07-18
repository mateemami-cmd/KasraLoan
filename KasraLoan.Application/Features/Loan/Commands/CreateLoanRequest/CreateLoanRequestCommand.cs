using KasraLoan.Application.DTOs.Loans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestCommand
        : IRequest<CreateLoanRequestResponse>
    {
        public CreateLoanRequestDto Request { get; set; } = null!;
    }
}