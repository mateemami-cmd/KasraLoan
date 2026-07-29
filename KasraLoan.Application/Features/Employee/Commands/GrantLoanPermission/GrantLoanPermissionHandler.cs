using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.GrantLoanPermission
{
    public class GrantLoanPermissionHandler : IRequestHandler<GrantLoanPermissionCommand, GrantLoanPermissionResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;

        public GrantLoanPermissionHandler(IEmployeeRepository employeeRepository, IEmployeeScoreRepository employeeScoreRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
        }

        public async Task<GrantLoanPermissionResponse> Handle(GrantLoanPermissionCommand request, CancellationToken cancellationToken)
        {
            var response = new GrantLoanPermissionResponse();

            foreach (var username in request.Request.Usernames)
            {
                var employee = await _employeeRepository.GetByUsernameAsync(username);

                if (employee == null)
                {
                    response.Results.Add(new GrantLoanPermissionResultItemDto
                    {
                        Username = username,
                        Success = false,
                        Message = "کارمندی با این نام کاربری یافت نشد."
                    });
                    continue;
                }

                var scoreRecord = await _employeeScoreRepository.GetByEmployeeIdAsync(employee.Id);

                if (scoreRecord == null)
                {
                    scoreRecord = new EmployeeScore
                    {
                        EmployeeId = employee.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _employeeScoreRepository.AddAsync(scoreRecord);
                }

                scoreRecord.HasLoanPermissionOverride = request.Request.Grant;
                scoreRecord.PermissionGrantedAt = request.Request.Grant ? DateTime.UtcNow : null;

                await _employeeScoreRepository.SaveChangesAsync();

                response.Results.Add(new GrantLoanPermissionResultItemDto
                {
                    Username = username,
                    Success = true,
                    Message = request.Request.Grant
                        ? "مجوز یک‌بارمصرف درخواست وام اعطا شد."
                        : "مجوز لغو شد."
                });
            }

            return response;
        }
    }
}