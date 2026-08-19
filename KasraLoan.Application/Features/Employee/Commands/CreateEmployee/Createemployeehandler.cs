using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.CreateEmployee
{
    public class CreateEmployeeHandler
        : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IUsernameGenerator _usernameGenerator;

        public CreateEmployeeHandler(
            IEmployeeRepository employeeRepository,
            IEmployeeScoreRepository employeeScoreRepository,
            IJobPositionRepository jobPositionRepository,
            ILoanTypeRepository loanTypeRepository,
            IPasswordHasher passwordHasher,
            IPasswordGenerator passwordGenerator,
            IUsernameGenerator usernameGenerator)
        {
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _jobPositionRepository = jobPositionRepository;
            _loanTypeRepository = loanTypeRepository;
            _passwordHasher = passwordHasher;
            _passwordGenerator = passwordGenerator;
            _usernameGenerator = usernameGenerator;
        }

        public async Task<CreateEmployeeResponse> Handle(
            CreateEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            var dto = request.Request;

            var role = UserRole.Employee;

            if (!string.IsNullOrWhiteSpace(dto.Role))
                Enum.TryParse(dto.Role, ignoreCase: true, out role);

            // ولیدیتور فقط اجباری‌بودن را چک می‌کند؛ وجود واقعی سمت اینجا تأیید می‌شود.
            Domain.Entities.JobPosition? jobPosition = null;
            if (dto.JobPositionId.HasValue)
            {
                jobPosition = await _jobPositionRepository.GetByIdAsync(dto.JobPositionId.Value);

                if (jobPosition == null)
                    throw new BusinessRuleException("سمت شغلی انتخاب‌شده یافت نشد.");

                if (!jobPosition.IsActive)
                    throw new BusinessRuleException("سمت شغلی انتخاب‌شده غیرفعال است.");
            }

            // برای کارمند، نام کاربری و شماره‌ی پرسنلی هر دو یک عددِ ۹ رقمیِ خودکارند
            // (سال استخدام + کد سمت + ترتیب) و یکسان‌اند؛ ادمین آن‌ها را دستی وارد می‌کند.
            string username;
            string personnelNumber;
            var isSeniorAdmin = false;
            int? managedLoanTypeId = null;
            if (role == UserRole.Admin)
            {
                if (string.IsNullOrWhiteSpace(dto.Username))
                    throw new BusinessRuleException("برای حساب ادمین باید نام کاربری وارد شود.");

                if (await _employeeRepository.UsernameExistsAsync(dto.Username))
                    throw new BusinessRuleException("این نام کاربری قبلاً استفاده شده است.");

                if (await _employeeRepository.PersonnelNumberExistsAsync(dto.PersonnelNumber))
                    throw new BusinessRuleException("این شماره پرسنلی قبلاً ثبت شده است.");

                username = dto.Username;
                personnelNumber = dto.PersonnelNumber;

                // نوع ادمین: ارشد (دسترسی کامل) یا ادمین وام (فقط یک نوع وام).
                isSeniorAdmin = dto.IsSeniorAdmin;
                if (!isSeniorAdmin)
                {
                    if (!dto.ManagedLoanTypeId.HasValue)
                        throw new BusinessRuleException("برای «ادمین وام» باید نوع وام مشخص شود.");

                    var managedLoanType = await _loanTypeRepository.GetByIdAsync(dto.ManagedLoanTypeId.Value);
                    if (managedLoanType == null)
                        throw new BusinessRuleException("نوع وامِ انتخاب‌شده یافت نشد.");

                    managedLoanTypeId = managedLoanType.Id;
                }
            }
            else
            {
                if (jobPosition == null)
                    throw new BusinessRuleException("برای ساخت نام کاربری کارمند، سمت شغلی الزامی است.");

                username = await _usernameGenerator.GenerateAsync(dto.HireDate, jobPosition);
                personnelNumber = username;
            }

            var temporaryPassword = _passwordGenerator.Generate();

            var employee = new Domain.Entities.Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PersonnelNumber = personnelNumber,
                Username = username,
                PasswordHash = _passwordHasher.Hash(temporaryPassword),
                HireDate = dto.HireDate,
                MarriageDate = dto.MarriageDate,
                IsActive = true,
                Role = role,
                IsSeniorAdmin = isSeniorAdmin,
                ManagedLoanTypeId = managedLoanTypeId,
                JobPositionId = dto.JobPositionId,
                MonthlySalary = dto.MonthlySalary
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            // بدون override دستی ساخته می‌شود؛ یعنی امتیاز از همان روز اول کاملاً
            // خودکار و بر اساس سابقه‌ی کار (HireDate) محاسبه می‌شود.
            await _employeeScoreRepository.AddAsync(new EmployeeScore
            {
                EmployeeId = employee.Id,
                ManualOverrideScore = null,
                CreatedAt = DateTime.UtcNow
            });
            await _employeeScoreRepository.SaveChangesAsync();

            return new CreateEmployeeResponse
            {
                Id = employee.Id,
                Username = employee.Username,
                TemporaryPassword = temporaryPassword,
                Message = "کارمند با موفقیت ایجاد شد. این رمز موقت را فقط یک‌بار می‌بینید؛ آن را از طریق کانال امن به کارمند اطلاع دهید و به او بگویید در اولین ورود رمزش را تغییر دهد."
            };
        }
    }
}