using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PersonnelNumber { get; set; }

        public string Token { get; set; }

        public DateTime HireDate { get; set; }

        public DateTime? MarriageDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
