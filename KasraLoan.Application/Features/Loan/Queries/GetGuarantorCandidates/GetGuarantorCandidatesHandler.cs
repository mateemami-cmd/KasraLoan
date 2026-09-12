using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Enums;
using MediatR;

namespace KasraLoan.Application.Features.Loan.Queries.GetGuarantorCandidates
{
    public class GetGuarantorCandidatesHandler
        : IRequestHandler<GetGuarantorCandidatesQuery, List<GuarantorCandidateDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetGuarantorCandidatesHandler(
            IEmployeeRepository employeeRepository,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<GuarantorCandidateDto>> Handle(
            GetGuarantorCandidatesQuery request, CancellationToken cancellationToken)
        {
            var requesterId = _currentUserService.UserId;
            var search = request.Search?.Trim();

            var candidates = (await _employeeRepository.GetAllAsync())
                .Where(e => e.Role == UserRole.Employee
                    && e.IsActive
                    && !e.IsDeleted
                    && e.EmploymentStatus == EmploymentStatus.Active
                    && e.Id != requesterId);

            if (!string.IsNullOrEmpty(search))
            {
                candidates = candidates.Where(e =>
                    ($"{e.FirstName} {e.LastName}").Contains(search)
                    || (e.PersonnelNumber ?? "").Contains(search));
            }

            return candidates
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Take(20)
                .Select(e => new GuarantorCandidateDto
                {
                    Id = e.Id,
                    FullName = $"{e.FirstName} {e.LastName}".Trim(),
                    PersonnelNumber = e.PersonnelNumber ?? ""
                })
                .ToList();
        }
    }
}
