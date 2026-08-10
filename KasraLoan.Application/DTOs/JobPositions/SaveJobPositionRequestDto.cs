namespace KasraLoan.Application.DTOs.JobPositions
{
    /// <summary>ورودی مشترک ساخت و ویرایش سمت شغلی.</summary>
    public class SaveJobPositionRequestDto
    {
        public string Title { get; set; } = string.Empty;

        /// <summary>حقوق پایه‌ی ماهانه به تومان.</summary>
        public long BaseSalary { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
