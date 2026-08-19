using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    /// <inheritdoc cref="IInstallmentPaymentService"/>
    public class InstallmentPaymentService : IInstallmentPaymentService
    {
        private const int GatewaySessionMinutes = 15;

        private readonly IInstallmentPaymentRepository _paymentRepository;
        private readonly ILoanInstallmentRepository _installmentRepository;
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IPayrollCalendarService _payrollCalendar;
        private readonly IPaymentGateway _gateway;
        private readonly IFileStorageService _fileStorage;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public InstallmentPaymentService(
            IInstallmentPaymentRepository paymentRepository,
            ILoanInstallmentRepository installmentRepository,
            ILoanRequestRepository loanRequestRepository,
            IPayrollCalendarService payrollCalendar,
            IPaymentGateway gateway,
            IFileStorageService fileStorage,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _paymentRepository = paymentRepository;
            _installmentRepository = installmentRepository;
            _loanRequestRepository = loanRequestRepository;
            _payrollCalendar = payrollCalendar;
            _gateway = gateway;
            _fileStorage = fileStorage;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }



        // ───────────────── وضعیت قسط جاری ─────────────────

        public async Task<CurrentInstallmentDto> GetCurrentInstallmentAsync(Guid employeeId)
        {
            var installment = await FindNextUnpaidInstallmentAsync(employeeId);

            if (installment == null)
            {
                return new CurrentInstallmentDto
                {
                    HasDueInstallment = false,
                    WindowDescription = DescribeWindow()
                };
            }

            var active = await _paymentRepository.GetActiveForInstallmentAsync(installment.Id);

            return new CurrentInstallmentDto
            {
                HasDueInstallment = true,
                LoanInstallmentId = installment.Id,
                InstallmentNumber = installment.InstallmentNumber,
                Amount = installment.Amount,
                DueDate = installment.DueDate,
                DueDatePersian = _payrollCalendar.ToPersianDateString(installment.DueDate),
                IsSelectionWindowOpen = IsSelectionAllowed(installment),
                SelectedMethod = active?.Method.ToString(),
                PaymentStatus = active?.Status.ToString(),
                WindowDescription = DescribeWindow()
            };
        }

        // ───────────────── انتخاب روش ─────────────────

        public async Task<InstallmentPaymentDto> SelectMethodAsync(
            Guid installmentId,
            Guid employeeId,
            PaymentMethod method)
        {
            var installment = await LoadOwnedInstallmentAsync(installmentId, employeeId);

            GuardSelectionAllowed(installment);

            var payment = await ReplaceActiveSelectionAsync(installment, employeeId, method);

            // چک با ثبت اطلاعات و تصویر کامل می‌شود؛ اینجا فقط انتخاب ثبت شده است.
            payment.Status = InstallmentPaymentStatus.Selected;

            await _paymentRepository.SaveChangesAsync();

            return ToDto(payment, installment);
        }

        // ───────────────── چک ─────────────────

        public async Task<InstallmentPaymentDto> SubmitChequeAsync(
            Guid installmentId,
            Guid employeeId,
            SubmitChequeRequestDto info,
            byte[] imageBytes,
            string fileName,
            string contentType)
        {
            var installment = await LoadOwnedInstallmentAsync(installmentId, employeeId);

            GuardSelectionAllowed(installment);

            if (imageBytes == null || imageBytes.Length == 0)
                throw new BusinessRuleException("تصویر چک الزامی است.");

            if (string.IsNullOrWhiteSpace(info.ChequeNumber))
                throw new BusinessRuleException("شماره چک الزامی است.");

            var imageUrl = await _fileStorage.SaveFileAsync(imageBytes, fileName, contentType);

            var payment = await ReplaceActiveSelectionAsync(installment, employeeId, PaymentMethod.Cheque);

            payment.ChequeImageUrl = imageUrl;
            payment.ChequeNumber = info.ChequeNumber.Trim();
            payment.ChequeBankName = info.ChequeBankName?.Trim();
            payment.ChequeDate = info.ChequeDate;
            payment.Status = InstallmentPaymentStatus.AwaitingAdminApproval;

            await _paymentRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                employeeId,
                installment.LoanRequestId,
                "SubmitCheque",
                $"Cheque submitted for installment {installment.InstallmentNumber}, amount {installment.Amount}.");

            return ToDto(payment, installment);
        }

        public async Task<List<InstallmentPaymentDto>> GetPendingChequesAsync()
        {
            var pending = await _paymentRepository.GetPendingChequesAsync();

            // «ادمین وام» فقط چک‌های اقساطِ نوع وام خودش را می‌بیند.
            if (!_currentUserService.IsSeniorAdmin)
            {
                pending = pending
                    .Where(p => _currentUserService.CanManageLoanType(p.LoanInstallment.LoanRequest.LoanTypeId))
                    .ToList();
            }

            return pending.Select(p => ToDto(p, p.LoanInstallment, includeAdminFields: true)).ToList();
        }

        public async Task<InstallmentPaymentDto> ConfirmChequeAsync(Guid paymentId, Guid adminId)
        {
            var payment = await LoadPendingChequeAsync(paymentId);

            payment.Status = InstallmentPaymentStatus.Confirmed;
            payment.ConfirmedByAdminId = adminId;
            payment.ConfirmedAt = DateTime.UtcNow;

            await MarkInstallmentPaidAsync(payment.LoanInstallment, PaymentMethod.Cheque);

            await _paymentRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                payment.EmployeeId,
                payment.LoanInstallment.LoanRequestId,
                "ConfirmCheque",
                $"Cheque {payment.ChequeNumber} confirmed for installment {payment.LoanInstallment.InstallmentNumber}.");

            await _notificationService.SendAsync(
                payment.EmployeeId,
                "تأیید چک",
                $"چک شما برای قسط شماره {payment.LoanInstallment.InstallmentNumber} تأیید شد و قسط تسویه گردید.");

            return ToDto(payment, payment.LoanInstallment);
        }

        public async Task<InstallmentPaymentDto> RejectChequeAsync(
            Guid paymentId,
            Guid adminId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleException("دلیل رد چک الزامی است.");

            var payment = await LoadPendingChequeAsync(paymentId);

            payment.Status = InstallmentPaymentStatus.Rejected;
            payment.ConfirmedByAdminId = adminId;
            payment.ConfirmedAt = DateTime.UtcNow;
            payment.RejectReason = reason.Trim();

            await _paymentRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                payment.EmployeeId,
                payment.LoanInstallment.LoanRequestId,
                "RejectCheque",
                $"Cheque {payment.ChequeNumber} rejected. Reason: {reason.Trim()}");

            // رکورد رد‌شده فعال حساب نمی‌شود، پس قسط دوباره بدون انتخاب می‌ماند و
            // اگر تا قطعی‌شدن لیست حقوق کاری نکند، کسر از حقوق می‌شود.
            await _notificationService.SendAsync(
                payment.EmployeeId,
                "رد چک",
                $"چک شما برای قسط شماره {payment.LoanInstallment.InstallmentNumber} رد شد. " +
                $"دلیل: {reason.Trim()} — در صورت عدم اقدام، این قسط از حقوق شما کسر خواهد شد.");

            return ToDto(payment, payment.LoanInstallment);
        }

        // ───────────────── درگاه ─────────────────

        public async Task<GatewaySessionDto> StartGatewayPaymentAsync(Guid installmentId, Guid employeeId)
        {
            var installment = await LoadOwnedInstallmentAsync(installmentId, employeeId);

            var payment = await ReplaceActiveSelectionAsync(
                installment, employeeId, PaymentMethod.OnlineGateway);

            payment.Status = InstallmentPaymentStatus.Selected;
            payment.GatewayAuthority = Guid.NewGuid();
            payment.GatewayExpiresAt = DateTime.UtcNow.AddMinutes(GatewaySessionMinutes);

            await _paymentRepository.SaveChangesAsync();

            return BuildSession(payment, installment);
        }

        public async Task<GatewaySessionDto> GetGatewaySessionAsync(Guid authority)
        {
            var payment = await _paymentRepository.GetByAuthorityAsync(authority);

            if (payment == null)
                throw new KeyNotFoundException("نشست پرداخت یافت نشد.");

            GuardSessionUsable(payment);

            return BuildSession(payment, payment.LoanInstallment);
        }

        public async Task<InstallmentPaymentDto> CompleteGatewayPaymentAsync(
            Guid authority,
            GatewayPaymentRequestDto card)
        {
            var payment = await _paymentRepository.GetByAuthorityAsync(authority);

            if (payment == null)
                throw new KeyNotFoundException("نشست پرداخت یافت نشد.");

            GuardSessionUsable(payment);

            var result = _gateway.Authorize(new GatewayCardInput
            {
                CardNumber = card.CardNumber,
                Cvv2 = card.Cvv2,
                ExpiryMonth = card.ExpiryMonth,
                ExpiryYear = card.ExpiryYear,
                SecondPassword = card.SecondPassword
            });

            if (!result.IsSuccessful)
            {
                payment.Status = InstallmentPaymentStatus.Failed;
                payment.RejectReason = result.FailureReason;

                await _paymentRepository.SaveChangesAsync();

                throw new BusinessRuleException(result.FailureReason ?? "پرداخت ناموفق بود.");
            }

            payment.Status = InstallmentPaymentStatus.Confirmed;
            payment.GatewayRefId = _gateway.GenerateReferenceId();
            payment.ConfirmedAt = DateTime.UtcNow;

            await MarkInstallmentPaidAsync(payment.LoanInstallment, PaymentMethod.OnlineGateway);

            await _paymentRepository.SaveChangesAsync();

            await _auditLogService.LogAsync(
                payment.EmployeeId,
                payment.LoanInstallment.LoanRequestId,
                "GatewayPayment",
                $"Installment {payment.LoanInstallment.InstallmentNumber} paid online. Ref: {payment.GatewayRefId}");

            await _notificationService.SendAsync(
                payment.EmployeeId,
                "پرداخت موفق",
                $"قسط شماره {payment.LoanInstallment.InstallmentNumber} به مبلغ " +
                $"{payment.Amount:N0} تومان پرداخت شد. شماره پیگیری: {payment.GatewayRefId}");

            return ToDto(payment, payment.LoanInstallment);
        }

        // ───────────────── کمکی‌ها ─────────────────

        private async Task<LoanInstallment?> FindNextUnpaidInstallmentAsync(Guid employeeId)
        {
            var loans = await _loanRequestRepository.GetOpenLoansWithInstallmentsAsync(employeeId);

            return loans
                .SelectMany(l => l.LoanInstallments)
                .Where(i => !i.IsPaid)
                .OrderBy(i => i.DueDate)
                .FirstOrDefault();
        }

        private async Task<LoanInstallment> LoadOwnedInstallmentAsync(Guid installmentId, Guid employeeId)
        {
            var installment = await _installmentRepository.GetByIdWithLoanAsync(installmentId);

            if (installment == null)
                throw new KeyNotFoundException("قسط مورد نظر یافت نشد.");

            if (installment.LoanRequest.EmployeeId != employeeId)
                throw new ForbiddenAccessException("این قسط متعلق به شما نیست.");

            if (installment.IsPaid)
                throw new BusinessRuleException("این قسط قبلاً پرداخت شده است.");

            return installment;
        }

        private async Task<InstallmentPayment> ReplaceActiveSelectionAsync(
            LoanInstallment installment,
            Guid employeeId,
            PaymentMethod method)
        {
            var active = await _paymentRepository.GetActiveForInstallmentAsync(installment.Id);

            if (active != null)
            {
                if (active.Status == InstallmentPaymentStatus.Confirmed)
                    throw new BusinessRuleException("این قسط قبلاً پرداخت شده است.");

                if (active.Status == InstallmentPaymentStatus.AwaitingAdminApproval)
                    throw new BusinessRuleException(
                        "چک شما برای این قسط در انتظار بررسی ادمین است؛ تا تعیین تکلیف نمی‌توانید روش را عوض کنید.");

                // انتخاب قبلی هنوز قطعی نشده، پس کنار گذاشته می‌شود.
                active.Status = InstallmentPaymentStatus.Failed;
                active.RejectReason = "جایگزین شد با انتخاب جدید.";
            }

            var payment = new InstallmentPayment
            {
                Id = Guid.NewGuid(),
                LoanInstallmentId = installment.Id,
                EmployeeId = employeeId,
                Method = method,
                Amount = installment.Amount,
                Status = InstallmentPaymentStatus.Selected,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            return payment;
        }

        /// <summary>
        /// قسط را تسویه‌شده علامت می‌زند و اگر آخرین قسط بود، خود وام را می‌بندد.
        /// بستن وام تا امروز هیچ‌جا انجام نمی‌شد و وام‌های تمام‌شده در حالت
        /// Approved باقی می‌ماندند.
        /// </summary>
        private async Task MarkInstallmentPaidAsync(LoanInstallment installment, PaymentMethod method)
        {
            installment.IsPaid = true;
            installment.PaidAt = DateTime.UtcNow;
            installment.PaidMethod = method;

            var loan = installment.LoanRequest;

            if (loan == null)
                return;

            // عمداً از دیتابیس پرسیده می‌شود و نه از loan.LoanInstallments:
            // وقتی قسط با Include بارگذاری شده باشد، EF فقط همان یک قسط را داخل
            // مجموعه‌ی وام می‌گذارد، و «همه پرداخت شده‌اند» با اولین پرداخت درست
            // درمی‌آید — وام با یک قسط از دوازده قسط بسته می‌شد.
            var hasUnpaid = await _installmentRepository.HasOtherUnpaidInstallmentsAsync(
                loan.Id, installment.Id);

            if (hasUnpaid)
                return;

            loan.Status = LoanStatus.Paid;

            await _notificationService.SendAsync(
                loan.EmployeeId,
                "تسویه‌ی وام",
                "آخرین قسط وام شما پرداخت شد و وام به‌طور کامل تسویه گردید.");
        }

        /// <summary>
        /// انتخاب روش فقط داخل پنجره ممکن است — مگر قسط معوق باشد، که در آن
        /// صورت هر زمانی باید بشود پرداختش کرد.
        /// </summary>
        private bool IsSelectionAllowed(LoanInstallment installment)
        {
            var now = DateTime.UtcNow;

            if (installment.DueDate < now)
                return true;

            return _payrollCalendar.IsWithinPaymentMethodSelectionWindow(now);
        }

        private void GuardSelectionAllowed(LoanInstallment installment)
        {
            if (IsSelectionAllowed(installment))
                return;

            throw new BusinessRuleException(
                $"انتخاب روش پرداخت فقط {DescribeWindow()} امکان‌پذیر است. " +
                $"امروز {_payrollCalendar.ToPersianDateString(DateTime.UtcNow)} است. " +
                "در صورت عدم انتخاب، قسط از حقوق کسر می‌شود.");
        }

        private static void GuardSessionUsable(InstallmentPayment payment)
        {
            if (payment.Status == InstallmentPaymentStatus.Confirmed)
                throw new BusinessRuleException("این پرداخت قبلاً انجام شده است.");

            if (payment.Status is InstallmentPaymentStatus.Failed or InstallmentPaymentStatus.Rejected)
                throw new BusinessRuleException("این نشست پرداخت معتبر نیست.");

            if (payment.GatewayExpiresAt.HasValue && payment.GatewayExpiresAt.Value < DateTime.UtcNow)
                throw new BusinessRuleException("مهلت این نشست پرداخت به پایان رسیده است.");
        }

        private string DescribeWindow()
        {
            return "از روز ۲۵ هر ماه شمسی تا پایان همان ماه";
        }

        private GatewaySessionDto BuildSession(InstallmentPayment payment, LoanInstallment installment)
        {
            return new GatewaySessionDto
            {
                Authority = payment.GatewayAuthority!.Value,
                Amount = payment.Amount,
                InstallmentNumber = installment.InstallmentNumber,
                GatewayName = _gateway.Name,
                ExpiresAt = payment.GatewayExpiresAt ?? DateTime.UtcNow,
                RedirectUrl = $"/payment/gateway/{payment.GatewayAuthority}"
            };
        }

        private InstallmentPaymentDto ToDto(
            InstallmentPayment payment,
            LoanInstallment installment,
            bool includeAdminFields = false)
        {
            return new InstallmentPaymentDto
            {
                Id = payment.Id,
                LoanInstallmentId = payment.LoanInstallmentId,
                InstallmentNumber = installment?.InstallmentNumber ?? 0,
                Method = payment.Method.ToString(),
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                ChequeImageUrl = payment.ChequeImageUrl,
                ChequeNumber = payment.ChequeNumber,
                ChequeBankName = payment.ChequeBankName,
                ChequeDate = payment.ChequeDate,
                ChequeDatePersian = payment.ChequeDate.HasValue
                    ? _payrollCalendar.ToPersianDateString(payment.ChequeDate.Value)
                    : null,
                GatewayRefId = payment.GatewayRefId,
                RejectReason = payment.RejectReason,
                CreatedAt = payment.CreatedAt,
                EmployeeName = includeAdminFields && payment.Employee != null
                    ? $"{payment.Employee.FirstName} {payment.Employee.LastName}"
                    : null,
                LoanTypeName = includeAdminFields
                    ? installment?.LoanRequest?.LoanType?.Name
                    : null
            };
        }

        private async Task<InstallmentPayment> LoadPendingChequeAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetByIdWithInstallmentAsync(paymentId);

            if (payment == null)
                throw new KeyNotFoundException("رکورد پرداخت یافت نشد.");

            if (payment.Method != PaymentMethod.Cheque)
                throw new BusinessRuleException("این پرداخت از نوع چک نیست.");

            // «ادمین وام» فقط چک‌های اقساطِ نوع وام خودش را تأیید/رد می‌کند.
            if (!_currentUserService.CanManageLoanType(payment.LoanInstallment.LoanRequest.LoanTypeId))
                throw new BusinessRuleException("شما به این نوع وام دسترسی ندارید.");

            if (payment.Status != InstallmentPaymentStatus.AwaitingAdminApproval)
                throw new BusinessRuleException("این چک قبلاً بررسی شده است.");

            return payment;
        }
    }
}
