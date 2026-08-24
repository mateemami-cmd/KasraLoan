using KasraLoan.Application.DTOs.Auth;
using MediatR;

namespace KasraLoan.Application.Features.Authentication.RegisterVisit
{
    /// <summary>
    /// ثبتِ «ورود» برای یک تبِ تازه‌بازشده که با توکنِ موجود (auto-resume) وارد شده.
    /// یک نشستِ فعالِ جدید و یک ردیفِ تاریخچه‌ی ورود می‌سازد — تا باز کردنِ تبِ جدید هم
    /// مثلِ یک ورودِ واقعی حساب شود. با توکنِ فعلی احراز هویت می‌شود (Authorize).
    /// </summary>
    public class RegisterVisitCommand : IRequest<LoginResponseDto>
    {
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}
