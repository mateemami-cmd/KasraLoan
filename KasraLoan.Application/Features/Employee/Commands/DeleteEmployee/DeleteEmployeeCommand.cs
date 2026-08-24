using System;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.DeleteEmployee
{
    /// <summary>
    /// حذفِ نرمِ کارمند: از فهرست‌های عادی کنار می‌رود و نمی‌تواند وارد شود، ولی
    /// ردیفش و همه‌ی سوابقش (وام‌ها، اقساط، پرداخت‌ها، ...) در دیتابیس می‌ماند.
    /// </summary>
    public class DeleteEmployeeCommand : IRequest<DeleteEmployeeResponse>
    {
        public Guid EmployeeId { get; set; }
    }

    public class DeleteEmployeeResponse
    {
        public Guid EmployeeId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
