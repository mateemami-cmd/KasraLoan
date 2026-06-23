using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonnelNumber { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? MarriageDate { get; set; }
        public bool IsActive { get; set; }

        public UserRole Role { get; set; } = UserRole.Employee;

        public ICollection<EmployeeLoginToken> EmployeeLoginTokens { get; set; } = new List<EmployeeLoginToken>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}