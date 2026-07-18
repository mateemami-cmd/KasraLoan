using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<GetCurrentUserResponse>
    {
        //public Guid EmployeeId { get; set; }
    }
}