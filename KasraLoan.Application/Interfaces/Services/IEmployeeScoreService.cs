using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface IEmployeeScoreService
    {
        int MinimumScoreRequiredForLoan { get; }

        int CalculateMonthsEmployed(DateTime hireDate);

        int CalculateAutomaticScore(DateTime hireDate);

        int GetEffectiveScore(Employee employee, EmployeeScore? scoreRecord);
    }
}