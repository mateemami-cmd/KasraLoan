

namespace KasraLoan.Application.Common.Logging
{
    /// <summary>
    /// اینترفیس نشانه‌گذار (marker). هر Command/Query که این را implement کند،
    /// <see cref="Behaviors.LoggingBehavior{TRequest,TResponse}"/> محتوای آن را
    /// در لاگ نمی‌نویسد و فقط نام درخواست را ثبت می‌کند.
    ///
    /// روی هر درخواستی بگذارید که داده‌ی حساس حمل می‌کند: رمز عبور، اطلاعات کارت
    /// بانکی، رمز دوم، توکن و مانند این‌ها. لاگ‌ها روی دیسک (Logs/log-*.txt) نوشته
    /// می‌شوند و نباید چنین داده‌هایی داخلشان باشد.
    /// </summary>
    public interface ISensitiveRequest
    {
    }
}
