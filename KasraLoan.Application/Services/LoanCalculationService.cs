using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    public class LoanCalculationService : ILoanCalculationService
    {
        public long CalculateMaxLoan(int score)
        {
            return ((long)score * 10000) + 10000000;
        }

        public long CalculateMonthlyPayment(long totalAmount, int months)
        {
            if (months == 0) return 0;

            return totalAmount / months;
        }
    }
}