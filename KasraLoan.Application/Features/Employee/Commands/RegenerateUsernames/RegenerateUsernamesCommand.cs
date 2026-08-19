using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.RegenerateUsernames
{
    /// <summary>
    /// نام کاربری همه‌ی کارمندان را بر اساس الگوریتم «سال استخدام + کد سمت + ترتیب»
    /// از نو می‌سازد. عملیاتی یک‌بار‌مصرف برای اعمال الگوریتم روی داده‌ی موجود است،
    /// اما چون کاملاً قطعی است، اجرای دوباره‌اش نتیجه‌ی یکسان می‌دهد (idempotent).
    /// ادمین‌ها و کارمندان بدون سمت شغلی دست‌نخورده می‌مانند.
    /// </summary>
    public class RegenerateUsernamesCommand : IRequest<RegenerateUsernamesResponse>
    {
    }
}
