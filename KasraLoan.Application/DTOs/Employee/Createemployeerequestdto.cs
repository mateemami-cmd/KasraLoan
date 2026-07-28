using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Employee
{
    public class CreateEmployeeRequestDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public DateTime HireDate { get; set; }

        public DateTime? MarriageDate { get; set; }

        /// <summary>
        /// "Employee" یا "Admin". اگر ارسال نشود، پیش‌فرض "Employee" است.
        /// </summary>
        public string? Role { get; set; }
    }
}