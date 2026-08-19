using System;
using KasraLoan.Application.DTOs.Employee;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetAdminScope
{
    public class SetAdminScopeCommand : IRequest<SetAdminScopeResponse>
    {
        public Guid EmployeeId { get; set; }
        public SetAdminScopeRequestDto Request { get; set; } = null!;
    }

    public class SetAdminScopeResponse
    {
        public Guid EmployeeId { get; set; }
        public bool IsSeniorAdmin { get; set; }
        public int? ManagedLoanTypeId { get; set; }
        public string? ManagedLoanTypeName { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
