using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.JobPositions;
using KasraLoan.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.JobPositions.Commands.UpdateJobPosition
{
    public class UpdateJobPositionHandler
        : IRequestHandler<UpdateJobPositionCommand, JobPositionDto>
    {
        private readonly IJobPositionRepository _jobPositionRepository;

        public UpdateJobPositionHandler(IJobPositionRepository jobPositionRepository)
        {
            _jobPositionRepository = jobPositionRepository;
        }

        public async Task<JobPositionDto> Handle(
            UpdateJobPositionCommand request,
            CancellationToken cancellationToken)
        {
            var position = await _jobPositionRepository.GetByIdAsync(request.Id);

            if (position == null)
                throw new KeyNotFoundException("سمت شغلی یافت نشد.");

            var dto = request.Request;

            var title = dto.Title.Trim();

            if (await _jobPositionRepository.TitleExistsAsync(title, excludeId: position.Id))
                throw new BusinessRuleException("سمت شغلی دیگری با این عنوان وجود دارد.");

            // غیرفعال کردن سمتی که هنوز کارمند دارد جلوی ثبت نام‌های بعدی را نمی‌گیرد
            // بلکه داده‌ی موجود را بی‌صاحب می‌کند؛ پس اجازه داده نمی‌شود.
            if (!dto.IsActive && position.IsActive
                && await _jobPositionRepository.HasEmployeesAsync(position.Id))
            {
                throw new BusinessRuleException(
                    "این سمت شغلی هنوز کارمند دارد و نمی‌توان غیرفعالش کرد. " +
                    "ابتدا سمت کارمندان مربوطه را تغییر دهید.");
            }

            position.Title = title;
            position.BaseSalary = dto.BaseSalary;
            position.IsActive = dto.IsActive;

            await _jobPositionRepository.SaveChangesAsync();

            var counts = await _jobPositionRepository.GetEmployeeCountsAsync();

            return new JobPositionDto
            {
                Id = position.Id,
                Title = position.Title,
                BaseSalary = position.BaseSalary,
                IsActive = position.IsActive,
                EmployeeCount = counts.TryGetValue(position.Id, out var count) ? count : 0
            };
        }
    }
}
