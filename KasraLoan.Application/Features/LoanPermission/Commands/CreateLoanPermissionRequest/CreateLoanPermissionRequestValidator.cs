using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Commands.CreateLoanPermissionRequest
{
    public class CreateLoanPermissionRequestValidator : AbstractValidator<CreateLoanPermissionRequestCommand>
    {
        public CreateLoanPermissionRequestValidator()
        {
            RuleFor(x => x.Request.LoanTypeId).GreaterThan(0);

            RuleFor(x => x.Request.Reason)
                .NotEmpty().WithMessage("نوشتن دلیل درخواست الزامی است.")
                .MaximumLength(1000);
        }
    }
}
