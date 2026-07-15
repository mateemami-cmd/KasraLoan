using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Token { get; set; } = string.Empty;
    }
}