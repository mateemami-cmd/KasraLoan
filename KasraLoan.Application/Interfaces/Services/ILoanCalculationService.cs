using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface ILoanCalculationService
    {
        long CalculateMaxLoan(int score);
        long CalculateMonthlyPayment(long totalAmount, int months);
    }
}