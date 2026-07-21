using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public class GetAuditLogsQuery
        : IRequest<List<GetAuditLogsResponse>>
    {
    }
}