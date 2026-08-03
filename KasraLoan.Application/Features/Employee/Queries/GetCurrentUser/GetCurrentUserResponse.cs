using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Queries.GetCurrentUser
{
    public class GetCurrentUserResponse
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PersonnelNumber { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public List<string> AdditionalPhoneNumbers { get; set; } = new();

        public string? Email { get; set; }

        public int Score { get; set; }

        public string Role { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }
    }
}