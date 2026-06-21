using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs;

namespace KasraLoan.Application.Services.Auth
{
    public interface IAuthService
    {
        Task<String?> LoginByToken(string token);
    }
}