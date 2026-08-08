using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.DeleteProfilePicture
{
    public class DeleteProfilePictureHandler
        : IRequestHandler<DeleteProfilePictureCommand, DeleteProfilePictureResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProfilePictureHandler(
            IEmployeeRepository employeeRepository,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _currentUserService = currentUserService;
        }

        public async Task<DeleteProfilePictureResponse> Handle(
            DeleteProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            employee.ProfilePictureUrl = null;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new DeleteProfilePictureResponse
            {
                Message = "عکس پروفایل حذف شد."
            };
        }
    }
}
