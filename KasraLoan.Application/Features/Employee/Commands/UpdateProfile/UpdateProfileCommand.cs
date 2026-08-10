using KasraLoan.Application.Common.Logging;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.UpdateProfile
{
    // ISensitiveRequest: NewPassword خام دارد و نباید در لاگ نوشته شود.
    public class UpdateProfileCommand : IRequest<UpdateProfileResponse>, ISensitiveRequest
    {
        public UpdateProfileRequestDto Request { get; set; } = null!;
    }
}