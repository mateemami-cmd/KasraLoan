namespace KasraLoan.Application.DTOs.Employee
{
    public class SetAccountStatusRequestDto
    {
        /// <summary>true یعنی حساب فعال شود، false یعنی غیرفعال.</summary>
        public bool IsActive { get; set; }
    }
}
