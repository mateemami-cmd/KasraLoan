using MediatR;
using System;
using System.Collections.Generic;

namespace KasraLoan.Application.Features.Employee.Queries.GetEmploymentStatusHistory
{
    public class GetEmploymentStatusHistoryQuery
        : IRequest<List<EmploymentStatusHistoryItemDto>>
    {
        public Guid EmployeeId { get; set; }
    }

    public class EmploymentStatusHistoryItemDto
    {
        public Guid Id { get; set; }

        public string FromStatus { get; set; } = string.Empty;

        public string ToStatus { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public Guid ChangedByAdminId { get; set; }

        public DateTime ChangedAt { get; set; }

        public string ChangedAtPersian { get; set; } = string.Empty;
    }
}
