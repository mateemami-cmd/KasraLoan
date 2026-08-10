using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Queries.GetEmploymentStatusHistory
{
    public class GetEmploymentStatusHistoryHandler
        : IRequestHandler<GetEmploymentStatusHistoryQuery, List<EmploymentStatusHistoryItemDto>>
    {
        private readonly IEmploymentStatusChangeRepository _statusChangeRepository;
        private readonly IPayrollCalendarService _payrollCalendar;

        public GetEmploymentStatusHistoryHandler(
            IEmploymentStatusChangeRepository statusChangeRepository,
            IPayrollCalendarService payrollCalendar)
        {
            _statusChangeRepository = statusChangeRepository;
            _payrollCalendar = payrollCalendar;
        }

        public async Task<List<EmploymentStatusHistoryItemDto>> Handle(
            GetEmploymentStatusHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var changes = await _statusChangeRepository
                .GetByEmployeeIdAsync(request.EmployeeId);

            return changes.Select(x => new EmploymentStatusHistoryItemDto
            {
                Id = x.Id,
                FromStatus = x.FromStatus.ToString(),
                ToStatus = x.ToStatus.ToString(),
                Reason = x.Reason,
                ChangedByAdminId = x.ChangedByAdminId,
                ChangedAt = x.ChangedAt,
                ChangedAtPersian = _payrollCalendar.ToPersianDateString(x.ChangedAt)
            })
            .ToList();
        }
    }
}
