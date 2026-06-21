using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class EmployeeLoginToken
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string Token { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public Employee Employee { get; set; }
    }
}
