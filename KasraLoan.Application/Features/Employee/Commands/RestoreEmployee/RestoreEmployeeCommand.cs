using System;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.RestoreEmployee
{
    /// <summary>بازگرداندنِ کارمندِ حذف‌شده. به‌صورتِ «غیرفعال» برمی‌گردد؛ ادمین در
    /// صورت نیاز جداگانه حسابش را فعال می‌کند.</summary>
    public class RestoreEmployeeCommand : IRequest<RestoreEmployeeResponse>
    {
        public Guid EmployeeId { get; set; }
    }

    public class RestoreEmployeeResponse
    {
        public Guid EmployeeId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
