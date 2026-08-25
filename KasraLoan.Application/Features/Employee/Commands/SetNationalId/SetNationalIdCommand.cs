using System;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetNationalId
{
    /// <summary>تعیین/ویرایشِ کد ملیِ یک کارمند (برای جایگزینیِ مقادیرِ موقتِ کاربرانِ قدیمی).</summary>
    public class SetNationalIdCommand : IRequest<SetNationalIdResponse>
    {
        public Guid EmployeeId { get; set; }
        public SetNationalIdRequestDto Request { get; set; } = null!;
    }

    public class SetNationalIdResponse
    {
        public Guid EmployeeId { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
