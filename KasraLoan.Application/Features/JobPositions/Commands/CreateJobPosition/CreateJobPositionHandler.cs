using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.JobPositions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.JobPositions.Commands.CreateJobPosition
{
    public class CreateJobPositionHandler
        : IRequestHandler<CreateJobPositionCommand, JobPositionDto>
    {
        private readonly IJobPositionRepository _jobPositionRepository;

        public CreateJobPositionHandler(IJobPositionRepository jobPositionRepository)
        {
            _jobPositionRepository = jobPositionRepository;
        }

        public async Task<JobPositionDto> Handle(
            CreateJobPositionCommand request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request;

            var title = dto.Title.Trim();

            if (await _jobPositionRepository.TitleExistsAsync(title))
                throw new BusinessRuleException("سمت شغلی با این عنوان قبلاً ثبت شده است.");

            var position = new JobPosition
            {
                Title = title,
                BaseSalary = dto.BaseSalary,
                IsActive = dto.IsActive
            };

            await _jobPositionRepository.AddAsync(position);
            await _jobPositionRepository.SaveChangesAsync();

            return new JobPositionDto
            {
                Id = position.Id,
                Title = position.Title,
                BaseSalary = position.BaseSalary,
                IsActive = position.IsActive,
                EmployeeCount = 0
            };
        }
    }
}
