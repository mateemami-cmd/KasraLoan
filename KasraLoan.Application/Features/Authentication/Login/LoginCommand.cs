using KasraLoan.Application.Common.Logging;
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
    // ISensitiveRequest: رمز عبور خام دارد و نباید در لاگ نوشته شود.
    public class LoginCommand : IRequest<LoginResponseDto>, ISensitiveRequest
    {
        public LoginRequestDto LoginRequest { get; set; } = null!;
    }
}