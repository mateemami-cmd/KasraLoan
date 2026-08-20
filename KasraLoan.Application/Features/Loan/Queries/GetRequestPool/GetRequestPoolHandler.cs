using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;

namespace KasraLoan.Application.Features.Loan.Queries.GetRequestPool
{
    public class GetRequestPoolHandler
        : IRequestHandler<GetRequestPoolQuery, GetRequestPoolResponse>
    {
        // برای این نمای دموی مدیریتی، همه‌ی درخواست‌ها یکجا خوانده و در حافظه ادغام
        // می‌شوند. حجم داده کوچک است؛ اگر روزی بزرگ شد، صفحه‌بندی سمت سرور اضافه می‌شود.
        private const int FetchAll = 100000;

        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ILoanPermissionRequestRepository _permissionRequestRepository;

        public GetRequestPoolHandler(
            ILoanRequestRepository loanRequestRepository,
            ILoanPermissionRequestRepository permissionRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
            _permissionRequestRepository = permissionRequestRepository;
        }

        public async Task<GetRequestPoolResponse> Handle(
            GetRequestPoolQuery request,
            CancellationToken cancellationToken)
        {
            var loans = await _loanRequestRepository.GetPagedAsync(1, FetchAll, null, null);
            var permissions = await _permissionRequestRepository.GetPagedAsync(1, FetchAll, null);

            var items = new List<RequestPoolItemDto>();

            items.AddRange(loans.Select(l => new RequestPoolItemDto
            {
                Id = l.Id,
                Category = "Loan",
                CategoryLabel = "درخواست وام",
                LoanTypeId = l.LoanTypeId,
                LoanTypeName = l.LoanType?.Name ?? string.Empty,
                EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}" : string.Empty,
                EmployeeUsername = l.Employee?.Username ?? string.Empty,
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt,
                Detail = l.RequestedAmount > 0 ? $"{l.RequestedAmount:N0} تومان" : null
            }));

            items.AddRange(permissions.Select(p => new RequestPoolItemDto
            {
                Id = p.Id,
                Category = "Permission",
                CategoryLabel = "درخواست مجوز وام",
                LoanTypeId = p.LoanTypeId,
                LoanTypeName = p.LoanType?.Name ?? string.Empty,
                EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : string.Empty,
                EmployeeUsername = p.Employee?.Username ?? string.Empty,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                Detail = p.Reason
            }));

            var ordered = items.OrderByDescending(x => x.CreatedAt).ToList();

            return new GetRequestPoolResponse
            {
                Items = ordered,
                TotalCount = ordered.Count
            };
        }
    }
}
