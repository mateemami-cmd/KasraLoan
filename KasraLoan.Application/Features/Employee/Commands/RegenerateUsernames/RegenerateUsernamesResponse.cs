using System;
using System.Collections.Generic;

namespace KasraLoan.Application.Features.Employee.Commands.RegenerateUsernames
{
    public class RegenerateUsernamesResponse
    {
        public int TotalEmployees { get; set; }
        public int ChangedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<UsernameChangeItem> Changes { get; set; } = new();
        public List<string> Skipped { get; set; } = new();
    }

    public class UsernameChangeItem
    {
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public int HireYear { get; set; }
        public string OldUsername { get; set; } = string.Empty;
        public string NewUsername { get; set; } = string.Empty;
        public string OldPersonnelNumber { get; set; } = string.Empty;
        public string NewPersonnelNumber { get; set; } = string.Empty;
    }
}
