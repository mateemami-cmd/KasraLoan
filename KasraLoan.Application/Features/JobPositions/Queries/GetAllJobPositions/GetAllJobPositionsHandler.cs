using KasraLoan.Application.DTOs.JobPositions;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.JobPositions.Queries.GetAllJobPositions
{
    public class GetAllJobPositionsHandler
        : IRequestHandler<GetAllJobPositionsQuery, GetAllJobPositionsResponse>
    {
        private readonly IJobPositionRepository _jobPositionRepository;

        public GetAllJobPositionsHandler(IJobPositionRepository jobPositionRepository)
        {
            _jobPositionRepository = jobPositionRepository;
        }

        public async Task<GetAllJobPositionsResponse> Handle(
            GetAllJobPositionsQuery request,
            CancellationToken cancellationToken)
        {
            var positions = await _jobPositionRepository.GetAllAsync(request.ActiveOnly);

            var counts = await _jobPositionRepository.GetEmployeeCountsAsync();

            var items = positions.Select(x => new JobPositionDto
            {
                Id = x.Id,
                Title = x.Title,
                BaseSalary = x.BaseSalary,
                IsActive = x.IsActive,
                EmployeeCount = counts.TryGetValue(x.Id, out var count) ? count : 0
            })
            .ToList();

            return new GetAllJobPositionsResponse { Items = items };
        }
    }
}
