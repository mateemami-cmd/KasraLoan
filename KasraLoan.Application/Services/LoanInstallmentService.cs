using KasraLoan.Application.Common.Results;
using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    public class LoanInstallmentService : ILoanInstallmentService
    {
        private readonly ILoanInstallmentRepository _repo;
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPayrollCalendarService _payrollCalendar;

        public LoanInstallmentService(
            ILoanInstallmentRepository repo,
            ILoanRequestRepository loanRequestRepository,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            IPayrollCalendarService payrollCalendar)
        {
            _repo = repo;
            _loanRequestRepository = loanRequestRepository;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _payrollCalendar = payrollCalendar;
        }

        public async Task<ApiResponse<List<LoanInstallmentDto>>> GetLoanInstallmentsAsync(Guid loanId)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(loanId);

            if (loan == null)
            {
                return new ApiResponse<List<LoanInstallmentDto>>
                {
                    IsSuccess = false,
                    Message = "وام یافت نشد."
                };
            }

            var isAdmin = string.Equals(
                _currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && loan.EmployeeId != _currentUserService.UserId)
            {
                return new ApiResponse<List<LoanInstallmentDto>>
                {
                    IsSuccess = false,
                    Message = "شما اجازه‌ی مشاهده‌ی اقساط این وام را ندارید."
                };
            }

            var installments = await _repo.GetByLoanIdAsync(loanId);

            var result = installments.Select(x => new LoanInstallmentDto
            {
                Id = x.Id,
                InstallmentNumber = x.InstallmentNumber,
                Amount = x.Amount,
                DueDate = x.DueDate,
                IsPaid = x.IsPaid
            }).ToList();

            return new ApiResponse<List<LoanInstallmentDto>>
            {
                IsSuccess = true,
                Data = result
            };
        }

        public async Task<ApiResponse<bool>> PayInstallmentAsync(Guid installmentId, Guid employeeId)
        {
            var installment = await _repo.GetByIdWithLoanAsync(installmentId);

            if (installment == null)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "قسط مورد نظر یافت نشد."
                };
            }
            if (installment.LoanRequest.EmployeeId != employeeId)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "شما اجازه پرداخت این قسط را ندارید."
                };
            }
            if (installment.IsPaid)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = "این قسط قبلاً پرداخت شده است."
                };
            }
            installment.IsPaid = true;

            await _repo.SaveChangesAsync();

            await _notificationService.SendAsync(employeeId, "پرداخت قسط", $"قسط شماره {installment.InstallmentNumber} با موفقیت پرداخت شد.");

            return new ApiResponse<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "قسط با موفقیت پرداخت شد."
            };
        }

        public async Task CreateInstallmentsAsync(Guid loanRequestId)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(loanRequestId);

            if (loan == null)
                throw new BusinessRuleException("وام یافت نشد");

            if (loan.ApprovedAmount <= 0)
                throw new BusinessRuleException("وام هنوز مبلغ تأیید شده ندارد");

            if (loan.InstallmentCount <= 0)
                throw new BusinessRuleException("تعداد اقساط نامعتبر است");

            // تقسیم صحیح (int / int) نباید انجام شود، چون باقیمانده گم می‌شود.
            // ابتدا مبلغ هر قسط را با گرد کردن به پایین (به عدد صحیح تومان) محاسبه می‌کنیم،
            // سپس باقیمانده‌ی حاصل از تقسیم را به قسط آخر اضافه می‌کنیم تا مجموع اقساط
            // همیشه دقیقاً برابر مبلغ کل قابل‌بازپرداخت باشد.
            var baseInstallmentAmount =
                Math.Floor((decimal)loan.TotalPayableAmount / loan.InstallmentCount);

            var remainder =
                loan.TotalPayableAmount - (baseInstallmentAmount * loan.InstallmentCount);

            // سررسیدها به روزِ پرداخت حقوق در ماه‌های شمسی گره می‌خورند، نه به
            // تاریخ تأیید وام. اگر به تاریخ تأیید گره می‌خوردند، وامی که روز ۱۷ام
            // تأیید شود اقساطش سررسید ۱۷ام می‌شد و کسر از حقوق و پنجره‌ی انتخاب
            // روش پرداخت هیچ‌کدام سر جای خودشان نمی‌نشستند.
            var dueDates = _payrollCalendar.GetInstallmentDueDatesUtc(
                loan.ApprovedAt ?? DateTime.UtcNow,
                loan.InstallmentCount);

            var installments = new List<LoanInstallment>();

            for (int i = 1; i <= loan.InstallmentCount; i++)
            {
                var isLastInstallment = i == loan.InstallmentCount;

                var amount = isLastInstallment
                    ? baseInstallmentAmount + remainder
                    : baseInstallmentAmount;

                installments.Add(new LoanInstallment
                {
                    Id = Guid.NewGuid(),

                    LoanRequestId = loan.Id,

                    InstallmentNumber = i,

                    Amount = amount,

                    DueDate = dueDates[i - 1],

                    IsPaid = false,

                    CreatedAt = DateTime.UtcNow
                });
            }

            await _repo.AddRangeAsync(installments);

            await _repo.SaveChangesAsync();
        }
    }
}