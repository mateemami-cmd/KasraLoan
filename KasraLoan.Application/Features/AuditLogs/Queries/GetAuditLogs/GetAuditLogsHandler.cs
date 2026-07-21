using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public class GetAuditLogsHandler
        : IRequestHandler<GetAuditLogsQuery, List<GetAuditLogsResponse>>
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public GetAuditLogsHandler(
            IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<List<GetAuditLogsResponse>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var logs = await _auditLogRepository.GetAllAsync();

            return logs.Select(x => new GetAuditLogsResponse
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                LoanRequestId = x.LoanRequestId,
                Action = x.Action,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            }).ToList();
        }
    }
}