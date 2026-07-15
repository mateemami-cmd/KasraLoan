using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services.Auth
{
    public interface IJwtService
    {
        string GenerateToken(Guid employeeId, string firstName, string personnelNumber, string role);
    }
}