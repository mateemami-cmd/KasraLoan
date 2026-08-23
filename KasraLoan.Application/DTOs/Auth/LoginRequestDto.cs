using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// شناسه‌ی ثابتِ دستگاه/مرورگر (GUID ساخته‌شده سمت کلاینت). اگر برای همین
        /// دستگاه نشستِ فعالی باشد، همان به‌روز می‌شود؛ وگرنه نشستِ جدید ساخته می‌شود.
        /// </summary>
        public string? DeviceId { get; set; }
    }
}