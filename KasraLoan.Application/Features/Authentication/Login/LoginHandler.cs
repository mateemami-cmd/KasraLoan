using KasraLoan.Application.DTOs.Auth;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Authentication.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtService _jwtService;

        public LoginHandler(
            IAuthRepository authRepository,
            IJwtService jwtService)
        {
            _authRepository = authRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var employee =
                await _authRepository.GetEmployeeByLoginTokenAsync(request.LoginRequest.Token);

            if (employee == null)
                throw new UnauthorizedAccessException("Invalid Token");

            var jwt = _jwtService.GenerateToken(
                employee.Id,
                employee.FirstName,
                employee.PersonnelNumber ?? "",
                employee.Role.ToString());

            return new LoginResponseDto
            {
                AccessToken = jwt,
                ExpireAt = DateTime.UtcNow.AddMinutes(60)
            };
        }
    }
}