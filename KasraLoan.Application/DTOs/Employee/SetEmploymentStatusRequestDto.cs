namespace KasraLoan.Application.DTOs.Employee
{
    /// <summary>
    /// ورودی تغییر وضعیت اشتغال. عمداً اندپوینت جدایی دارد و در
    /// <see cref="AdminUpdateEmployeeRequestDto"/> نیست — دقیقاً به همان دلیلی که
    /// امتیاز هم آنجا نیست: تغییر وضعیت اشتغال یک رویداد مالی است، پنجره‌ی زمانی
    /// دارد و باید لاگ شود؛ نباید به‌عنوان عارضه‌ی جانبیِ ویرایش شماره‌تلفن اتفاق بیفتد.
    /// </summary>
    public class SetEmploymentStatusRequestDto
    {
        /// <summary>"Active" یا "Terminated"</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>دلیل تغییر: استعفا، پایان قرارداد، بازگشت به کار و ...</summary>
        public string Reason { get; set; } = string.Empty;
    }
}
