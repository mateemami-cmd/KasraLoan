using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Queries.GetCurrentUser
{
    public class GetCurrentUserHandler
    : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        public Task<GetCurrentUserResponse> Handle(
            GetCurrentUserQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}