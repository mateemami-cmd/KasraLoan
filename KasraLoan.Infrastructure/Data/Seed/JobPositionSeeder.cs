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
            new JobPosition { Title = "دواپس",                BaseSalary = 60_000_000 },
            new JobPosition { Title = "توسعه‌دهنده بک‌اند",     BaseSalary = 50_000_000 },
            new JobPosition { Title = "توسعه‌دهنده فرانت‌اند",  BaseSalary = 40_000_000 },
            new JobPosition { Title = "پلتفرم",                BaseSalary = 45_000_000 },
            new JobPosition { Title = "هوش مصنوعی",            BaseSalary = 50_000_000 },
            new JobPosition { Title = "محصول",                 BaseSalary = 40_000_000 },
            new JobPosition { Title = "پشتیبانی",              BaseSalary = 40_000_000 },
        };

        public static async Task SeedAsync(KasraLoanDbContext context)
        {
            foreach (var position in Positions)
            {
                var exists = await context.JobPositions
                    .AnyAsync(x => x.Title == position.Title);

                if (!exists)
                    context.JobPositions.Add(position);
            }

            await context.SaveChangesAsync();
        }
    }
}
