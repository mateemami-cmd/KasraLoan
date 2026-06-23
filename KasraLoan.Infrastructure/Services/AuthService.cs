using KasraLoan.Application.DTOs;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly KasraLoanDbContext _context;

        private readonly IConfiguration _configuration;

        public AuthService(KasraLoanDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public AuthService(KasraLoanDbContext context)
        {
            _context = context;
        }

        public async Task<string?> LoginByToken(string token)
        {
            var loginToken = await _context.EmployeeLoginTokens
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Token == token && x.IsActive);

            if (loginToken?.Employee == null)
                return null;

            var employee = loginToken.Employee;

            var jwtSettings = _configuration.GetSection("JwtSettings");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
        new Claim(ClaimTypes.Name, employee.FirstName),
        new Claim("PersonnelNumber", employee.PersonnelNumber ?? ""),
        new Claim(ClaimTypes.Role, employee.Role.ToString())
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(jwtSettings["ExpireMinutes"] ?? "60")
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}