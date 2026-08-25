using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Common;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using EmployeeEntity = KasraLoan.Domain.Entities.Employee;

namespace KasraLoan.Application.Features.Authentication.ResetByIdentity
{
    public class VerifyIdentityHandler : IRequestHandler<VerifyIdentityCommand, VerifyIdentityResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public VerifyIdentityHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<VerifyIdentityResponse> Handle(VerifyIdentityCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(request.Request.Username);
            IdentityGuard.EnsureMatch(employee, request.Request.NationalId);

            return new VerifyIdentityResponse { Message = "هویت تأیید شد." };
        }
    }

    /// <summary>بررسیِ مشترکِ نام کاربری + کد ملی، برای هر دو مرحله.</summary>
    internal static class IdentityGuard
    {
        public static void EnsureMatch(EmployeeEntity? employee, string? nationalId)
        {
            const string invalid = "نام کاربری یا کد ملی نادرست است.";

            var stored = NationalIdValidator.Normalize(employee?.NationalId);
            var entered = NationalIdValidator.Normalize(nationalId);

            if (employee == null
                || employee.IsDeleted
                || !employee.IsActive
                || stored.Length == 0
                || stored != entered)
            {
                throw new BusinessRuleException(invalid);
            }
        }
    }
}
