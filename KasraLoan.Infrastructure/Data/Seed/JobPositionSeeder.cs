using KasraLoan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Data.Seed
{
    public static class JobPositionSeeder
    {
        /// <summary>
        /// سمت‌های شغلی اولیه و حقوق پایه‌ی هرکدام (تومان).
        /// این‌ها فقط مقدار اولیه‌اند؛ ادمین می‌تواند از پنل تغییرشان دهد و
        /// سید هرگز حقوقی را که ادمین دستی عوض کرده بازنویسی نمی‌کند.
        /// </summary>
        private static readonly List<JobPosition> Positions = new()
        {
            new JobPosition { Title = "دواپس",                Code = "01", BaseSalary = 60_000_000 },
            new JobPosition { Title = "توسعه‌دهنده بک‌اند",     Code = "02", BaseSalary = 50_000_000 },
            new JobPosition { Title = "هوش مصنوعی",            Code = "03", BaseSalary = 50_000_000 },
            new JobPosition { Title = "پلتفرم",                Code = "04", BaseSalary = 45_000_000 },
            new JobPosition { Title = "توسعه‌دهنده فرانت‌اند",  Code = "05", BaseSalary = 40_000_000 },
            new JobPosition { Title = "محصول",                 Code = "06", BaseSalary = 40_000_000 },
            new JobPosition { Title = "پشتیبانی",              Code = "07", BaseSalary = 40_000_000 },
        };

        public static async Task SeedAsync(KasraLoanDbContext context)
        {
            foreach (var position in Positions)
            {
                var existing = await context.JobPositions
                    .FirstOrDefaultAsync(x => x.Title == position.Title);

                if (existing == null)
                {
                    context.JobPositions.Add(position);
                }
                // کد سمت داده‌ی مرجع است، نه ورودی ادمین؛ اگر خالی یا متفاوت بود
                // با مقدار درست پر می‌شود (مثلاً برای سمت‌هایی که قبل از افزودن
                // ستون Code ساخته شده‌اند). حقوق پایه عمداً بازنویسی نمی‌شود.
                else if (existing.Code != position.Code)
                {
                    existing.Code = position.Code;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
