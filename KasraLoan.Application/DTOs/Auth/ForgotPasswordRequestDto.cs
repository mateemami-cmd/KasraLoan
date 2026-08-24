namespace KasraLoan.Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        /// <summary>ایمیلی که کاربر با آن ثبت شده؛ رمزِ موقت به همین ارسال می‌شود.</summary>
        public string Email { get; set; } = string.Empty;
    }
}
