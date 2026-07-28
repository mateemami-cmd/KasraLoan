using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Features.Employee.Commands.CreateEmployee;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KasraLoan.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequestDto request)
        {
            var result = await _mediator.Send(new CreateEmployeeCommand
            {
                Request = request
            });

            return Ok(result);
        }
    }
}