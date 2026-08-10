namespace KasraLoan.Application.DTOs.Loans
{
    /// <summary>بدنه‌ی اختیاری رد وام؛ فقط برای دلیل رد.</summary>
    public class RejectLoanRequestDto
    {
        public string? RejectReason { get; set; }
    }
}
