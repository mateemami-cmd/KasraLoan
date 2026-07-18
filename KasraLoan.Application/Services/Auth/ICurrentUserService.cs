using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services.Auth
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }

        string? FirstName { get; }

        string? PersonnelNumber { get; }

        string? Role { get; }

        bool IsAuthenticated { get; }
    }
}