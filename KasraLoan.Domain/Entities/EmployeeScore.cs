using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Entities
{
    public class EmployeeScore
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }

        public int Score { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Employee? Employee { get; set; }
    }
}