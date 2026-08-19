using System;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetAccountStatus
{
    /// <summary>
    /// فعال/غیرفعال کردن «حساب کاربری» کارمند (IsActive). این با «وضعیت اشتغال»
    /// فرق دارد: اشتغال یعنی کارمند مشغول کار است یا نه؛ حساب کاربری یعنی اجازه‌ی
    /// ورود به سامانه دارد یا نه. حساب غیرفعال نمی‌تواند وارد شود و وام بگیرد.
    /// </summary>
    public class SetAccountStatusCommand : IRequest<SetAccountStatusResponse>
    {
        public Guid EmployeeId { get; set; }
        public SetAccountStatusRequestDto Request { get; set; } = null!;
    }
}
