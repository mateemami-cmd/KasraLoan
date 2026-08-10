using KasraLoan.Application.DTOs.JobPositions;
using MediatR;

namespace KasraLoan.Application.Features.JobPositions.Commands.UpdateJobPosition
{
    public class UpdateJobPositionCommand : IRequest<JobPositionDto>
    {
        public int Id { get; set; }

        public SaveJobPositionRequestDto Request { get; set; } = null!;
    }
}
