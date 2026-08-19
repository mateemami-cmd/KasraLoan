using System;

namespace KasraLoan.Application.Features.Employee.Commands.SetAccountStatus
{
    public class SetAccountStatusResponse
    {
        public Guid EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
