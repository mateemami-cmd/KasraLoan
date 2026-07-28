using KasraLoan.Application.Features.Loan.Queries.GetLoanInstallments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KasraLoan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoanInstallmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoanInstallmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{loanId:guid}")]
    public async Task<IActionResult> GetLoanInstallments(Guid loanId)
    {
        var result = await _mediator.Send(new GetLoanInstallmentsQuery(loanId));

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}