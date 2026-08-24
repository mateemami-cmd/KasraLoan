namespace KasraLoan.Application.DTOs.Auth
{
    /// <summary>
    /// تعیینِ رمزِ جدید بعد از ورود با رمزِ موقت. چون رمزِ فعلی موقت است و کاربر
    /// آن را نمی‌داند، فقط رمزِ جدید گرفته می‌شود (نه رمزِ فعلی).
    /// </summary>
    public class ResetPasswordRequestDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
