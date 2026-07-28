using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.CreateEmployee
{
    public class CreateEmployeeResponse
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// رمز موقت فقط همین یک‌بار در پاسخ برگردانده می‌شود و در دیتابیس
        /// به‌صورت خام ذخیره نمی‌شود (فقط هش آن ذخیره می‌شود).
        /// ادمین باید این را از طریق یک کانال امن به کارمند اطلاع دهد.
        /// </summary>
        public string TemporaryPassword { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}