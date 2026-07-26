using System;

namespace KasraLoan.Application.Common.Exceptions
{
    /// <summary>
    /// زمانی پرتاب می‌شود که کاربر احراز هویت شده است،
    /// اما اجازه‌ی دسترسی به منبع درخواستی را ندارد (مثلاً وام کارمند دیگر).
    /// به HTTP 403 Forbidden نگاشت می‌شود.
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException()
            : base("شما اجازه‌ی دسترسی به این منبع را ندارید.")
        {
        }

        public ForbiddenAccessException(string message) : base(message)
        {
        }
    }
}