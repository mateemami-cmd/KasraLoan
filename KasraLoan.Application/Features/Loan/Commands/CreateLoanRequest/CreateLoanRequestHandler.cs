using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.LoanRules;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using KasraLoan.Domain.Validation;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestHandler : IRequestHandler<CreateLoanRequestCommand, CreateLoanRequestResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ILoanTypeRepository _loanTypeRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeScoreRepository _employeeScoreRepository;
        private readonly IEmployeeScoreService _employeeScoreService;
        private readonly ILoanRuleEngine _loanRuleEngine;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILoanDocumentRepository _loanDocumentRepository;

        public CreateLoanRequestHandler(
        ILoanRequestRepository loanRequestRepository,
        ILoanTypeRepository loanTypeRepository,
        IEmployeeRepository employeeRepository,
        IEmployeeScoreRepository employeeScoreRepository,
        IEmployeeScoreService employeeScoreService,
        ILoanRuleEngine loanRuleEngine,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IFileStorageService fileStorageService,
        ILoanDocumentRepository loanDocumentRepository)
        {
            _loanRequestRepository = loanRequestRepository;
            _loanTypeRepository = loanTypeRepository;
            _employeeRepository = employeeRepository;
            _employeeScoreRepository = employeeScoreRepository;
            _employeeScoreService = employeeScoreService;
            _loanRuleEngine = loanRuleEngine;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
            _loanDocumentRepository = loanDocumentRepository;
        }

        public async Task<CreateLoanRequestResponse> Handle(CreateLoanRequestCommand request, CancellationToken cancellationToken)
        {

            var employeeId = _currentUserService.UserId;

            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            // کارمندی که دیگر مشغول به کار نیست، وام جدید نمی‌گیرد. حسابش باز می‌ماند
            // تا بتواند اقساط وام قبلی‌اش را ببیند و بپردازد، ولی درخواست تازه ممنوع است.
            if (employee.EmploymentStatus != Domain.Enums.EmploymentStatus.Active)
                throw new BusinessRuleException(
                    "وضعیت اشتغال شما فعال نیست و امکان ثبت درخواست وام جدید وجود ندارد.");

            // اگر همین الان یک وام فعال (Pending/Approved/Active) داشته باشد،
            // نباید بتواند درخواست وام جدیدی ثبت کند (حتی با مجوز استثنایی).
            var hasActiveLoan = await _loanRequestRepository.HasActiveLoanAsync(employeeId);

            if (hasActiveLoan)
                throw new BusinessRuleException(
                    "شما در حال حاضر یک وام فعال دارید و تا زمان تسویه‌ی کامل آن، نمی‌توانید درخواست وام جدیدی ثبت کنید.");

            var loanType = await _loanTypeRepository
                .GetByIdAsync(request.Request.LoanTypeId);

            if (loanType == null)
                throw new KeyNotFoundException("Loan type not found");


            var employeeScore = await _employeeScoreRepository
                .GetByEmployeeIdAsync(employeeId);


            if (employeeScore == null)
                throw new KeyNotFoundException("Employee score not found");

            var effectiveScore = _employeeScoreService.GetEffectiveScore(employee, employeeScore);

            var hasPermissionOverride = employeeScore.HasLoanPermissionOverride;

            // اگر ادمین مجوز استثنایی داده باشد، امتیاز کارمند برای همین یک درخواست
            // به‌اندازه‌ی حداقل لازم در نظر گرفته می‌شود (بدون این‌که امتیاز واقعی‌اش تغییر کند).
            var scoreForEligibilityCheck = hasPermissionOverride
                ? Math.Max(effectiveScore, _employeeScoreService.MinimumScoreRequiredForLoan)
                : effectiveScore;

            // تاریخ عقدِ واردشده در فرم باید پیش از اجرای قانون روی کارمند بنشیند،
            // چون MarriageLoanRule همان را می‌خواند. اگر بعد از قانون ست می‌شد،
            // کارمندی که تاریخ عقدش در پروفایل نبود همیشه رد می‌شد — حتی وقتی
            // همان لحظه در فرم واردش کرده بود.
            var marriageDateWasAdded = ApplyMarriageDate(employee, loanType, request.Request);

            var context = new LoanRuleContext
            {
                Employee = employee,
                LoanType = loanType,
                RequestedAmount = request.Request.RequestedAmount,
                EmployeeScore = scoreForEligibilityCheck,
                RequestedInstallmentCount = request.Request.InstallmentCount
            };


            var ruleResult = _loanRuleEngine.Evaluate(context);


            if (!ruleResult.IsAllowed)
            {
                throw new BusinessRuleException(ruleResult.Message);
            }

            // مبلغ تأییدشده هرگز نباید بیشتر از مبلغ درخواستی کارمند باشد،
            // حتی اگر سقف مجاز قانون بیشتر از آن باشد.
            // نکته: مبالغ از نوع long هستند؛ cast به int برای وام‌های بزرگ‌تر از
            // حدود ۲.۱ میلیارد تومان سرریز (OverflowException) می‌داد، پس long استفاده می‌شود.
            var approvedAmount = Math.Min(
                request.Request.RequestedAmount,
                (long)ruleResult.MaxAllowedAmount);

            // تعداد اقساط درخواستی کارمند را می‌پذیریم، اما هرگز بیشتر از
            // سقف مجاز همان نوع وام نخواهد بود.
            var installmentCount = Math.Min(
                request.Request.InstallmentCount,
                ruleResult.MaxInstallments);

            // مدرک قبل از ساخته شدن درخواست بررسی می‌شود: وامی که مدرک لازم دارد
            // نباید حتی ثبت شود اگر مدرکی همراهش نیست.
            ValidateAttachments(request.Attachments, ruleResult.RequiresDocument,
                ruleResult.RequiredDocumentDescription);

            var details = BuildDetails(loanType, request.Request);

            var loanRequest = new Domain.Entities.LoanRequest
            {
                Details = details,
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                LoanTypeId = loanType.Id,
                RequestedAmount = request.Request.RequestedAmount,
                ApprovedAmount = approvedAmount,
                InstallmentCount = installmentCount,
                AnnualFeePercent = ruleResult.AnnualFeePercent,
                Status = Domain.Enums.LoanStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                RequiresDocument = ruleResult.RequiresDocument,
                RequiredDocumentDescription = ruleResult.RequiredDocumentDescription
            };


            await _loanRequestRepository.AddAsync(loanRequest);

            // تاریخ عقد تازه‌واردشده همراه خودِ درخواست ذخیره می‌شود تا دفعه‌ی بعد
            // کارمند دوباره از او پرسیده نشود.
            if (marriageDateWasAdded)
                await _employeeRepository.UpdateAsync(employee);

            await _loanRequestRepository.SaveChangesAsync();

            await SaveAttachmentsAsync(loanRequest.Id, request.Attachments);

            // مجوز استثنایی یک‌بارمصرف است: همین که با موفقیت استفاده شد، مصرف می‌شود.
            if (hasPermissionOverride)
            {
                employeeScore.HasLoanPermissionOverride = false;
                employeeScore.PermissionGrantedAt = null;
                await _employeeScoreRepository.SaveChangesAsync();
            }

            await _notificationService.SendAsync(
                loanRequest.EmployeeId,
                "ثبت درخواست وام",
                "درخواست وام شما با موفقیت ثبت شد و در انتظار بررسی است.");

            return new CreateLoanRequestResponse
            {
                LoanRequestId = loanRequest.Id,
                Message = "درخواست وام با موفقیت ثبت شد",
                RequiresDocument = loanRequest.RequiresDocument,
                RequiredDocumentDescription = loanRequest.RequiredDocumentDescription
            };
        }

        /// <summary>حداکثر تعداد فایل پیوست برای یک درخواست.</summary>
        private const int MaxAttachments = 2;

        private const long MaxAttachmentBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

        private static void ValidateAttachments(
            List<LoanAttachment> attachments,
            bool requiresDocument,
            string? requiredDescription)
        {
            if (requiresDocument && attachments.Count == 0)
            {
                throw new BusinessRuleException(
                    $"برای این وام بارگذاری {requiredDescription ?? "مدرک"} الزامی است؛ " +
                    "بدون آن امکان ثبت درخواست وجود ندارد.");
            }

            if (attachments.Count > MaxAttachments)
                throw new BusinessRuleException($"حداکثر {MaxAttachments} فایل می‌توانید بارگذاری کنید.");

            foreach (var file in attachments)
            {
                if (file.Content.Length == 0)
                    throw new BusinessRuleException("فایل خالی است.");

                if (file.Content.Length > MaxAttachmentBytes)
                    throw new BusinessRuleException($"حجم «{file.FileName}» بیشتر از ۵ مگابایت است.");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    throw new BusinessRuleException(
                        $"فرمت «{file.FileName}» مجاز نیست. فقط JPG، PNG و PDF پذیرفته می‌شود.");
                }
            }
        }

        private async Task SaveAttachmentsAsync(Guid loanRequestId, List<LoanAttachment> attachments)
        {
            if (attachments.Count == 0)
                return;

            foreach (var file in attachments)
            {
                var path = await _fileStorageService.SaveFileAsync(
                    file.Content, file.FileName, file.ContentType);

                await _loanDocumentRepository.AddAsync(new LoanDocument
                {
                    LoanRequestId = loanRequestId,
                    FileName = file.FileName,
                    FilePath = path,
                    UploadedAt = DateTime.UtcNow
                });
            }

            await _loanDocumentRepository.SaveChangesAsync();
        }

        /// <summary>
        /// جزئیات مخصوص نوع وام را می‌سازد و اعتبارسنجی می‌کند.
        /// انواعی که هنوز فرم اختصاصی ندارند، null برمی‌گردانند.
        /// </summary>
        /// <summary>
        /// اگر تاریخ عقد در پروفایل کارمند خالی است و فرم آن را آورده، روی کارمند
        /// می‌نشیند. تاریخ عقدِ از قبل ثبت‌شده هرگز از طریق فرم وام بازنویسی
        /// نمی‌شود — تغییرش کار پروفایل و ادمین است، نه یک درخواست وام.
        /// </summary>
        private static bool ApplyMarriageDate(
            // نام کامل لازم است: namespaceی Features.Employee نام Employee را می‌پوشاند.
            Domain.Entities.Employee employee,
            LoanType loanType,
            CreateLoanRequestDto dto)
        {
            if (loanType.Type != LoanTypeEnum.MarriageLoan)
                return false;

            if (employee.MarriageDate.HasValue)
                return false;

            var provided = dto.Marriage?.MarriageDate;

            if (provided == null)
                return false;

            employee.MarriageDate = DateTime.SpecifyKind(provided.Value.Date, DateTimeKind.Utc);

            return true;
        }

        private static LoanDetails? BuildDetails(LoanType loanType, CreateLoanRequestDto dto)
        {
            if (loanType.Type == LoanTypeEnum.MarriageLoan)
                return BuildMarriageDetails(dto);

            if (loanType.Type != LoanTypeEnum.TravelLoan)
                return null;

            if (dto.Travel == null)
                throw new BusinessRuleException("اطلاعات سفر را کامل کنید.");

            var travel = dto.Travel;

            if (string.IsNullOrWhiteSpace(travel.Destination))
                throw new BusinessRuleException("مقصد سفر را وارد کنید.");

            if (!Enum.TryParse<TravelDestinationType>(
                    travel.DestinationType, ignoreCase: true, out var destinationType))
            {
                throw new BusinessRuleException("نوع مقصد معتبر نیست.");
            }

            if (travel.EndDate.Date <= travel.StartDate.Date)
                throw new BusinessRuleException("تاریخ پایان سفر باید بعد از تاریخ شروع باشد.");

            if (travel.StartDate.Date < DateTime.UtcNow.Date)
                throw new BusinessRuleException("تاریخ شروع سفر نمی‌تواند در گذشته باشد.");

            return new LoanDetails
            {
                Travel = new TravelLoanDetails
                {
                    DestinationType = destinationType,
                    Destination = travel.Destination.Trim(),
                    StartDate = DateTime.SpecifyKind(travel.StartDate.Date, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(travel.EndDate.Date, DateTimeKind.Utc),
                    Notes = string.IsNullOrWhiteSpace(travel.Notes) ? null : travel.Notes.Trim()
                }
            };
        }

        private static LoanDetails BuildMarriageDetails(CreateLoanRequestDto dto)
        {
            var marriage = dto.Marriage
                ?? throw new BusinessRuleException("اطلاعات ازدواج را کامل کنید.");

            if (string.IsNullOrWhiteSpace(marriage.SpouseFirstName)
                || string.IsNullOrWhiteSpace(marriage.SpouseLastName))
            {
                throw new BusinessRuleException("نام و نام خانوادگی همسر را وارد کنید.");
            }

            var nationalId = NationalId.Normalize(marriage.SpouseNationalId);

            if (!NationalId.IsValid(nationalId))
                throw new BusinessRuleException("کد ملی همسر معتبر نیست.");

            return new LoanDetails
            {
                Marriage = new MarriageLoanDetails
                {
                    SpouseFirstName = marriage.SpouseFirstName.Trim(),
                    SpouseLastName = marriage.SpouseLastName.Trim(),
                    SpouseNationalId = nationalId,
                    Notes = string.IsNullOrWhiteSpace(marriage.Notes) ? null : marriage.Notes.Trim()
                }
            };
        }
    }
}