using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Services
{
    public class EmployeeScoreService : IEmployeeScoreService
    {
        private const int PointsPerMonth = 100;

        public int MinimumScoreRequiredForLoan => 600;

        public int CalculateMonthsEmployed(DateTime hireDate)
        {
            var today = DateTime.UtcNow.Date;
            var hire = hireDate.Date;

            if (hire > today)
                return 0;

            var months = ((today.Year - hire.Year) * 12) + (today.Month - hire.Month);

            if (today.Day < hire.Day)
                months--;

            return Math.Max(months, 0);
        }

        public int CalculateAutomaticScore(DateTime hireDate)
        {
            return CalculateMonthsEmployed(hireDate) * PointsPerMonth;
        }

        public int GetEffectiveScore(Employee employee, EmployeeScore? scoreRecord)
        {
            if (scoreRecord?.ManualOverrideScore.HasValue == true)
                return scoreRecord.ManualOverrideScore.Value;

            return CalculateAutomaticScore(employee.HireDate);
        }
    }
}