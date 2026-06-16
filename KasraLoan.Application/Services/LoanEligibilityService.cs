using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    public class LoanEligibilityService : ILoanEligibilityService
    {
        public bool IsEligible(int score, int minScore)
        {
            return score >= minScore;
        }
    }
}