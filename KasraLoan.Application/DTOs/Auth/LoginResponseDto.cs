using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpireAt { get; set; }

        /// <summary>
        /// اگر true باشد، کاربر به سقفِ نشست‌ها رسیده و ورود انجام نشد؛ باید یکی از
        /// <see cref="Sessions"/> را برای قطع انتخاب کند و دوباره تلاش کند. در این
        /// حالت توکن‌ها خالی‌اند.
        /// </summary>
        public bool RequiresSessionChoice { get; set; }

        public List<SessionDto>? Sessions { get; set; }
    }
}