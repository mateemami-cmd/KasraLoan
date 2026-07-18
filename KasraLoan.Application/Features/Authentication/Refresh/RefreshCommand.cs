using KasraLoan.Application.DTOs.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Refresh
{
    public class RefreshCommand : IRequest<LoginResponseDto>
    {
        public RefreshTokenRequestDto Request { get; set; } = null!;
    }
}