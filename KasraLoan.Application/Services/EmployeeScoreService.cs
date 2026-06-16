using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    public class EmployeeScoreService : IEmployeeScoreService
    {
        public int CalculateScore(int totalMonths)
        {
            return totalMonths * 100;
        }
    }
}
