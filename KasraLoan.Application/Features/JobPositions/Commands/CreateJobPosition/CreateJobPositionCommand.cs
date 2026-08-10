using KasraLoan.Application.DTOs.JobPositions;
using MediatR;

namespace KasraLoan.Application.Features.JobPositions.Commands.CreateJobPosition
{
    public class CreateJobPositionCommand : IRequest<JobPositionDto>
    {
        public SaveJobPositionRequestDto Request { get; set; } = null!;
    }
}
