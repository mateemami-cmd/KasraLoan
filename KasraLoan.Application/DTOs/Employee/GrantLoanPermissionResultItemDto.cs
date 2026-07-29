using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.DTOs.Employee
{
    public class GrantLoanPermissionResultItemDto
    {
        public string Username { get; set; } = string.Empty;

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class GrantLoanPermissionResponse
    {
        public List<GrantLoanPermissionResultItemDto> Results { get; set; } = new();
    }
}