using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<Employee?> LoginWithTokenAsync(string token);
        Task<Employee?> GetEmployeeByIdAsync(int id);
    }
}