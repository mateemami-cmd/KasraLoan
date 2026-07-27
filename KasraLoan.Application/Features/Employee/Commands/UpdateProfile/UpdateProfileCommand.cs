using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateProfile
{
    public class UpdateProfileCommand : IRequest<UpdateProfileResponse>
    {
        public UpdateProfileRequestDto Request { get; set; } = null!;
    }
}