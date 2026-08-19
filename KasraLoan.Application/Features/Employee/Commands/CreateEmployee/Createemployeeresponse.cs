using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.CreateEmployee
{
    public class CreateEmployeeResponse
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}