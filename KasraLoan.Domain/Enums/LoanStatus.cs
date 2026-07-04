using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Domain.Enums
{
    public enum LoanStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Active = 3,
        Paid = 4,
        Closed = 5
    }
}