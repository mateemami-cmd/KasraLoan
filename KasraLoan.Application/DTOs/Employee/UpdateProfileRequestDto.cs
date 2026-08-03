using System;

namespace KasraLoan.Application.DTOs.Employee
{
    /// <summary>
    /// فقط فیلدهایی که خودِ کارمند اجازه‌ی تغییرشان را دارد.
    /// نام، نام‌خانوادگی، یوزرنیم، شماره پرسنلی و نقش، از این طریق قابل تغییر نیستند.
    /// هر فیلد اختیاری است: فقط مقادیری که ارسال شوند به‌روزرسانی می‌شوند.
    /// </summary>
    public class UpdateProfileRequestDto
    {
        public string? NewPassword { get; set; }

        public string? PhoneNumber { get; set; }

        public string? SecondaryPhoneNumber { get; set; }

        public string? Email { get; set; }
    }
}