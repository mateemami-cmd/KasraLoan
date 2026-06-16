using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface ILoanCalculationService
    {
        int CalculateMaxLoan(int score);
        int CalculateMonthlyPayment(int totalAmount, int months);
    }
}