using KasraLoan.Application.Services.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User =>
            _httpContextAccessor.HttpContext?.User;

        public Guid UserId =>
            Guid.TryParse(
                User?.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id)
                    ? id
                    : Guid.Empty;

        public string? FirstName =>
            User?.FindFirstValue(ClaimTypes.Name);

        public string? PersonnelNumber =>
            User?.FindFirst("PersonnelNumber")?.Value;

        public string? Role =>
            User?.FindFirst(ClaimTypes.Role)?.Value;

        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated ?? false;
    }
}