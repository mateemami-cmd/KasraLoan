using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IEmployeeScoreRepository
    {
        Task<int> GetScoreByEmployeeIdAsync(Guid employeeId);
    }
}