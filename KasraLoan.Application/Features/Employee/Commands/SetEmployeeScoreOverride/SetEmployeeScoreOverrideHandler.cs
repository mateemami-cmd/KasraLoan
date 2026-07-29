using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KasraLoan.Application.DTOs.Employee;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Entities;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.SetEmployeeScoreOverride
{
    public class SetEmployeeScoreOverrideHandler : IRequestHandler<SetEmployeeScoreOverrideCommand, EmployeeScoreResponseDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly IEmployeeScoreService _employeeScoreService;

        public SetEmployeeScoreOverrideHandler(
            IEmployeeRepository employeeRepository,
            IEmployeeScoreRepository employeeScoreRepository,
            IEmployeeScoreService employeeScoreService)
        {
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _employeeScoreService = employeeScoreService;
        }

        public async Task<EmployeeScoreResponseDto> Handle(SetEmployeeScoreOverrideCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);

            if (employee == null)
                throw new KeyNotFoundException("کارمند یافت نشد.");

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

            // request.Request.Score == null یعنی override پاک شود و به حالت خودکار برگردد.
            scoreRecord.ManualOverrideScore = request.Request.Score;
            scoreRecord.OverriddenAt = request.Request.Score.HasValue
                ? DateTime.UtcNow
                : null;

            await _employeeScoreRepository.SaveChangesAsync();

            var monthsEmployed = _employeeScoreService.CalculateMonthsEmployed(employee.HireDate);
            var automaticScore = _employeeScoreService.CalculateAutomaticScore(employee.HireDate);
            var effectiveScore = _employeeScoreService.GetEffectiveScore(employee, scoreRecord);

            return new EmployeeScoreResponseDto
            {
                EmployeeId = employee.Id,
                MonthsEmployed = monthsEmployed,
                AutomaticScore = automaticScore,
                ManualOverrideScore = scoreRecord.ManualOverrideScore,
                EffectiveScore = effectiveScore,
                IsOverridden = scoreRecord.ManualOverrideScore.HasValue,
                MinimumScoreRequiredForLoan = _employeeScoreService.MinimumScoreRequiredForLoan,
                HasActiveLoanPermission = scoreRecord.HasLoanPermissionOverride
            };
        }
    }
}