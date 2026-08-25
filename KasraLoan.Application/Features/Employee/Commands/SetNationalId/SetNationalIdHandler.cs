using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetNationalId
{
    public class SetNationalIdHandler : IRequestHandler<SetNationalIdCommand, SetNationalIdResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public SetNationalIdHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<SetNationalIdResponse> Handle(SetNationalIdCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

            employee.NationalId = KasraLoan.Application.Common.NationalIdValidator.Normalize(request.Request.NationalId);

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new SetNationalIdResponse
            {
                EmployeeId = employee.Id,
                NationalId = employee.NationalId,
                Message = "کد ملی به‌روزرسانی شد."
            };
        }
    }
}
