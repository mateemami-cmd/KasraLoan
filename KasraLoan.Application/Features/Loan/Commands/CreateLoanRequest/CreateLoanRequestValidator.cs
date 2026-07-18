using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestValidator
        : AbstractValidator<CreateLoanRequestCommand>
    {
        public CreateLoanRequestValidator()
        {
            RuleFor(x => x.Request.LoanTypeId)
                .GreaterThan(0);

            RuleFor(x => x.Request.RequestedAmount)
                .GreaterThan(0);

            RuleFor(x => x.Request.InstallmentCount)
                .GreaterThan(0);
        }
    }
}