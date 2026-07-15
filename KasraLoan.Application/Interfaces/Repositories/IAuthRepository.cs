using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<Employee?> GetEmployeeByLoginTokenAsync(string token);
    }
}