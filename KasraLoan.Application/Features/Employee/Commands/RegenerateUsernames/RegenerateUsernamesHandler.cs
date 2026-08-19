using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Enums;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Commands.RegenerateUsernames
{
    public class RegenerateUsernamesHandler
        : IRequestHandler<RegenerateUsernamesCommand, RegenerateUsernamesResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IUsernameGenerator _usernameGenerator;

        public RegenerateUsernamesHandler(
            IEmployeeRepository employeeRepository,
            IJobPositionRepository jobPositionRepository,
            IUsernameGenerator usernameGenerator)
        {
            _employeeRepository = employeeRepository;
            _jobPositionRepository = jobPositionRepository;
            _usernameGenerator = usernameGenerator;
        }

        public async Task<RegenerateUsernamesResponse> Handle(
            RegenerateUsernamesCommand request,
            CancellationToken cancellationToken)
        {
            var all = await _employeeRepository.GetAllAsync();
            var positions = (await _jobPositionRepository.GetAllAsync(activeOnly: false))
                .ToDictionary(p => p.Id);

            var response = new RegenerateUsernamesResponse { TotalEmployees = all.Count };

            // فقط کارمندان دارای سمت شغلی. ادمین‌ها و هرکس بدون سمت دست‌نخورده می‌مانند.
            var targets = all
                .Where(e => e.Role == UserRole.Employee && e.JobPositionId.HasValue)
                .Select(e => new
                {
                    Employee = e,
                    Position = positions.TryGetValue(e.JobPositionId!.Value, out var p) ? p : null,
                    HireYear = _usernameGenerator.GetHireYear(e.HireDate)
                })
                .ToList();

            // سمت بدون کد یا سمت پیدانشده را نمی‌توان شماره‌گذاری کرد؛ رد می‌شوند.
            foreach (var skipped in targets.Where(t => t.Position == null || string.IsNullOrWhiteSpace(t.Position.Code)))
            {
                response.Skipped.Add($"{skipped.Employee.FirstName} {skipped.Employee.LastName} ({skipped.Employee.Username})");
            }

            var groups = targets
                .Where(t => t.Position != null && !string.IsNullOrWhiteSpace(t.Position.Code))
                .GroupBy(t => new { t.HireYear, PositionId = t.Position!.Id });

            foreach (var group in groups)
            {
                // ترتیب پایدار: هرکه زودتر استخدام شده شماره‌ی کوچک‌تر می‌گیرد؛
                // Id فقط برای شکستن تساوی تاریخ استفاده می‌شود تا نتیجه قطعی بماند.
                var ordered = group
                    .OrderBy(t => t.Employee.HireDate)
                    .ThenBy(t => t.Employee.Id)
                    .ToList();

                var sequence = 1;
                foreach (var item in ordered)
                {
                    var newIdentifier = _usernameGenerator.Compose(
                        item.HireYear, item.Position!.Code, sequence);
                    sequence++;

                    // نام کاربری و شماره‌ی پرسنلی هر دو باید همین کد شوند؛ اگر هیچ‌کدام
                    // فرق نداشت، این کارمند از قبل درست است و رد می‌شود.
                    if (item.Employee.Username == newIdentifier
                        && item.Employee.PersonnelNumber == newIdentifier)
                        continue;

                    response.Changes.Add(new UsernameChangeItem
                    {
                        EmployeeId = item.Employee.Id,
                        FullName = $"{item.Employee.FirstName} {item.Employee.LastName}",
                        PositionTitle = item.Position.Title,
                        HireYear = item.HireYear,
                        OldUsername = item.Employee.Username,
                        NewUsername = newIdentifier,
                        OldPersonnelNumber = item.Employee.PersonnelNumber,
                        NewPersonnelNumber = newIdentifier
                    });

                    item.Employee.Username = newIdentifier;
                    item.Employee.PersonnelNumber = newIdentifier;
                }
            }

            response.ChangedCount = response.Changes.Count;
            response.SkippedCount = response.Skipped.Count;

            if (response.ChangedCount > 0)
                await _employeeRepository.SaveChangesAsync();

            return response;
        }
    }
}
