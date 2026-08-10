namespace KasraLoan.Application.DTOs.JobPositions
{
    public class JobPositionDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>حقوق پایه‌ی ماهانه به تومان.</summary>
        public long BaseSalary { get; set; }

        public bool IsActive { get; set; }

        /// <summary>تعداد کارمندانی که در حال حاضر این سمت را دارند.</summary>
        public int EmployeeCount { get; set; }
    }
}
