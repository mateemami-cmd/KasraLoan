using KasraLoan.Application.DTOs.LoanPermission;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Queries.GetAllLoanPermissionRequests
{
    public class GetAllLoanPermissionRequestsHandler
        : IRequestHandler<GetAllLoanPermissionRequestsQuery, GetAllLoanPermissionRequestsResponse>
    {
        private readonly ILoanPermissionRequestRepository _permissionRequestRepository;

        public GetAllLoanPermissionRequestsHandler(
            ILoanPermissionRequestRepository permissionRequestRepository)
        {
            _permissionRequestRepository = permissionRequestRepository;
        }

        public async Task<GetAllLoanPermissionRequestsResponse> Handle(
            GetAllLoanPermissionRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var requests = await _permissionRequestRepository.GetPagedAsync(
                request.Page, request.PageSize, request.Status);

            var totalCount = await _permissionRequestRepository.GetPagedCountAsync(request.Status);

            var items = requests.Select(x => new LoanPermissionRequestListItemDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null
                    ? $"{x.Employee.FirstName} {x.Employee.LastName}"
                    : string.Empty,
                EmployeeUsername = x.Employee?.Username ?? string.Empty,
                LoanTypeId = x.LoanTypeId,
                LoanTypeName = x.LoanType?.Name ?? string.Empty,
                Reason = x.Reason,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt,
                ReviewedAt = x.ReviewedAt,
                AdminResponse = x.AdminResponse
            })
                .ToList();

            var totalPages = request.PageSize > 0
                ? (int)Math.Ceiling(totalCount / (double)request.PageSize)
                : 0;

            return new GetAllLoanPermissionRequestsResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
