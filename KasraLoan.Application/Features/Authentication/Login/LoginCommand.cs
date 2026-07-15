using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.DTOs.Loans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Login
{
    public class LoginCommand : IRequest<LoginResponseDto>
    {
        public LoginRequestDto LoginRequest { get; set; } = null!;
    }
}