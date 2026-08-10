using KasraLoan.Application.Common.Payroll;
using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasraLoan.Application.Services
{
    /// <inheritdoc cref="ILoanSettlementService"/>
    public class LoanSettlementService : ILoanSettlementService
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly IPayrollCalendarService _payrollCalendar;
        private readonly PayrollCycleOptions _options;

        public LoanSettlementService(
            ILoanRequestRepository loanRequestRepository,
            IPayrollCalendarService payrollCalendar,
            IOptions<PayrollCycleOptions> options)
        {
            _loanRequestRepository = loanRequestRepository;
            _payrollCalendar = payrollCalendar;
            _options = options.Value;
        }

        public async Task<LoanOutstandingDto> GetOutstandingAsync(Guid loanRequestId)
        {
            var loan = await _loanRequestRepository.GetByIdAsync(loanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد.");

            return BuildOutstanding(loan);
        }

        public async Task<LoanSettlementDemandDto?> DemandSettlementForEmployeeAsync(
            Guid employeeId,
            string reason)
        {
            var openLoans = await _loanRequestRepository
                .GetOpenLoansWithInstallmentsAsync(employeeId);

            // فقط وام‌هایی که واقعاً مانده دارند. وامی که همه‌ی اقساطش پرداخت شده
            // ولی هنوز Closed نشده، نباید مطالبه شود.
            var loansWithBalance = openLoans
                .Where(x => CalculateOutstanding(x) > 0)
                .ToList();

            if (loansWithBalance.Count == 0)
                return null;

            var now = DateTime.UtcNow;

            var dueDate = now.AddDays(_options.SettlementGraceDays);

            var totalOutstanding = 0L;
            var remainingInstallments = 0;

            foreach (var loan in loansWithBalance)
            {
                var outstanding = CalculateOutstanding(loan);

                totalOutstanding += outstanding;

                remainingInstallments += loan.LoanInstallments.Count(i => !i.IsPaid);

                // اقساط عمداً حذف یا ادغام نمی‌شوند: سابقه‌ی زمان‌بندی باید بماند.
                // فقط وام علامت می‌خورد که کل مانده‌اش تا این تاریخ باید پرداخت شود.
                loan.SettlementDemandedAt = now;
                loan.SettlementDueDate = dueDate;
                loan.SettlementAmount = outstanding;
                loan.SettlementReason = reason;
            }

            await _loanRequestRepository.SaveChangesAsync();

            return new LoanSettlementDemandDto
            {
                LoanRequestIds = loansWithBalance.Select(x => x.Id).ToList(),
                TotalOutstandingAmount = totalOutstanding,
                RemainingInstallments = remainingInstallments,
                SettlementDueDate = dueDate,
                SettlementDueDatePersian = _payrollCalendar.ToPersianDateString(dueDate),
                Reason = reason
            };
        }

        private LoanOutstandingDto BuildOutstanding(LoanRequest loan)
        {
            var installments = loan.LoanInstallments ?? new List<LoanInstallment>();

            var paid = installments.Where(x => x.IsPaid).ToList();

            return new LoanOutstandingDto
            {
                LoanRequestId = loan.Id,
                TotalPayableAmount = loan.TotalPayableAmount,
                PaidAmount = (long)paid.Sum(x => x.Amount),
                OutstandingAmount = CalculateOutstanding(loan),
                TotalInstallments = installments.Count,
                PaidInstallments = paid.Count,
                RemainingInstallments = installments.Count - paid.Count,
                IsSettlementDemanded = loan.IsSettlementDemanded,
                SettlementDueDate = loan.SettlementDueDate,
                SettlementDueDatePersian = loan.SettlementDueDate.HasValue
                    ? _payrollCalendar.ToPersianDateString(loan.SettlementDueDate.Value)
                    : null,
                SettlementReason = loan.SettlementReason
            };
        }

        private static long CalculateOutstanding(LoanRequest loan)
        {
            if (loan.LoanInstallments == null)
                return 0;

            return (long)loan.LoanInstallments
                .Where(x => !x.IsPaid)
                .Sum(x => x.Amount);
        }
    }
}
