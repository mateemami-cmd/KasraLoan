using KasraLoan.Application.DTOs.LoanPermission;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.LoanPermission.Queries.GetMyLoanPermissionRequests
{
    public class GetMyLoanPermissionRequestsHandler
        : IRequestHandler<GetMyLoanPermissionRequestsQuery, GetMyLoanPermissionRequestsResponse>
    {
        private readonly ILoanPermissionRequestRepository _permissionRequestRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyLoanPermissionRequestsHandler(
            ILoanPermissionRequestRepository permissionRequestRepository,
            ICurrentUserService currentUserService)
        {
            _permissionRequestRepository = permissionRequestRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetMyLoanPermissionRequestsResponse> Handle(
            GetMyLoanPermissionRequestsQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = _currentUserService.UserId;

            var requests = await _permissionRequestRepository.GetByEmployeeIdAsync(employeeId);

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

            return new GetMyLoanPermissionRequestsResponse
            {
                Items = items
            };
        }
    }
}
