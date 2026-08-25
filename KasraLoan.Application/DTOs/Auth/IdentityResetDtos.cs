namespace KasraLoan.Application.DTOs.Auth
{
    /// <summary>مرحله‌ی اول فراموشیِ رمز: بررسیِ نام کاربری + کد ملی.</summary>
    public class VerifyIdentityRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
    }

    /// <summary>مرحله‌ی دوم: تعیینِ رمزِ جدید پس از تأییدِ نام کاربری + کد ملی.</summary>
    public class ResetByIdentityRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
