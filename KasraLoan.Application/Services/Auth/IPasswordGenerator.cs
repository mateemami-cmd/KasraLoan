using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services.Auth
{
    public interface IPasswordGenerator
    {
        /// <summary>
        /// یک رمز عبور تصادفی و امن (شامل حرف بزرگ، کوچک، عدد و کاراکتر خاص) تولید می‌کند.
        /// برای رمز موقت هنگام ایجاد حساب کارمند جدید استفاده می‌شود.
        /// </summary>
        string Generate(int length = 12);
    }
}
