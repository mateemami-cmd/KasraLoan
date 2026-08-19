using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Services.Auth;
using KasraLoan.Domain.Entities;

namespace KasraLoan.Infrastructure.Services
{
    public class UsernameGenerator : IUsernameGenerator
    {
        // PersianCalendar بدون حالت داخلی است و می‌شود یک نمونه‌ی مشترک داشت.
        private static readonly PersianCalendar Persian = new PersianCalendar();

        private const int UsernameLength = 9;
        private const int PrefixLength = 6; // YYYY(4) + CC(2)

        private readonly IEmployeeRepository _employeeRepository;

        public UsernameGenerator(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public int GetHireYear(DateTime hireDate) => Persian.GetYear(hireDate);

        public string Compose(int hireYear, string positionCode, int sequence)
            => $"{hireYear:0000}{positionCode}{sequence:000}";

        public async Task<string> GenerateAsync(DateTime hireDate, JobPosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            if (string.IsNullOrWhiteSpace(position.Code))
                throw new InvalidOperationException(
                    $"سمت «{position.Title}» کد ندارد؛ نمی‌توان نام کاربری ساخت.");

            var year = GetHireYear(hireDate);
            var prefix = $"{year:0000}{position.Code}"; // ۶ رقم اول: سال + کد سمت

            // شماره‌ی ترتیب = یکی بیشتر از بزرگ‌ترین شماره‌ی موجود در همین (سال + سمت).
            // «بزرگ‌ترین + ۱» انتخاب شده تا حذف یک کارمند باعث تکراری‌شدن شماره نشود.
            var all = await _employeeRepository.GetAllAsync();

            var maxSequence = all
                .Where(e => IsInGroup(e.Username, prefix))
                .Select(e => ParseSequence(e.Username))
                .DefaultIfEmpty(0)
                .Max();

            return Compose(year, position.Code, maxSequence + 1);
        }

        private static bool IsInGroup(string? username, string prefix)
            => username != null
               && username.Length == UsernameLength
               && username.StartsWith(prefix, StringComparison.Ordinal)
               && username.All(char.IsDigit);

        private static int ParseSequence(string username)
            => int.TryParse(username.Substring(PrefixLength), out var n) ? n : 0;
    }
}
