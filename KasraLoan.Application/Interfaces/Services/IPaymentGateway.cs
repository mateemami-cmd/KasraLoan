namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// درگاه پرداخت. الان فقط یک پیاده‌سازی نمادین دارد، ولی جریان کار همان
    /// جریان یک درگاه واقعی است: نشست باز می‌شود، کاربر به صفحه‌ی پرداخت می‌رود،
    /// و نتیجه سمت سرور تأیید می‌شود. وقتی زرین‌پال یا شاپرک اضافه شود، فقط
    /// همین اینترفیس پیاده‌سازی می‌شود و بقیه‌ی کد دست نمی‌خورد.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>نام درگاه، برای نمایش و لاگ.</summary>
        string Name { get; }

        /// <summary>
        /// آیا اطلاعات واردشده پرداخت را موفق می‌کند.
        /// اطلاعات کارت هرگز ذخیره یا لاگ نمی‌شوند؛ فقط اعتبارسنجی می‌شوند و
        /// دور ریخته می‌شوند.
        /// </summary>
        GatewayResult Authorize(GatewayCardInput input);

        /// <summary>شماره پیگیری برای یک پرداخت موفق.</summary>
        string GenerateReferenceId();
    }

    /// <summary>
    /// اطلاعات کارت. عمداً یک نوع جدا و کوتاه‌عمر است که هیچ‌وقت به انتیتی یا
    /// لاگ نمی‌رسد.
    /// </summary>
    public class GatewayCardInput
    {
        public string CardNumber { get; set; } = string.Empty;

        public string Cvv2 { get; set; } = string.Empty;

        public string ExpiryMonth { get; set; } = string.Empty;

        public string ExpiryYear { get; set; } = string.Empty;

        public string SecondPassword { get; set; } = string.Empty;
    }

    public class GatewayResult
    {
        public bool IsSuccessful { get; set; }

        public string? FailureReason { get; set; }

        public static GatewayResult Success() => new() { IsSuccessful = true };

        public static GatewayResult Failure(string reason) =>
            new() { IsSuccessful = false, FailureReason = reason };
    }
}
