using KasraLoan.Application.DTOs.JobPositions;
using System.Collections.Generic;

namespace KasraLoan.Application.Features.JobPositions.Queries.GetAllJobPositions
{
    public class GetAllJobPositionsResponse
    {
        public List<JobPositionDto> Items { get; set; } = new();
    }
}
