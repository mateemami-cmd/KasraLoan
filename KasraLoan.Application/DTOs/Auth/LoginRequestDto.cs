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
        /// اگر کاربر به سقفِ نشست‌ها رسیده باشد، شناسه‌ی نشستی که انتخاب کرده تا
        /// قطع شود و جا برای ورودِ جدید باز شود.
        /// </summary>
        public int? TerminateSessionId { get; set; }
    }
}