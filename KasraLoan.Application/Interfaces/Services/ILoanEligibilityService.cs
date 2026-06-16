using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface ILoanEligibilityService
    {
        bool IsEligible(int score, int minScore);
    }
}