using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Data.Seed
{
    public static class SeedIds
    {
        public static readonly Guid AdminAli =
            Guid.Parse("ac56f19c-8b8d-41ff-b47e-93d237fd24c4");

        /// <summary>کارمندِ دمو با امتیازِ خوب (سابقه‌ی بالا) — سناریوی درخواستِ موفق.</summary>
        public static readonly Guid EmployeeSaraGoodScore =
            Guid.Parse("b1a2c3d4-0001-4000-8000-000000000001");

        /// <summary>کارمندِ دمو با امتیازِ پایین (سابقه‌ی کم) — سناریوی رد شدن.</summary>
        public static readonly Guid EmployeeRezaLowScore =
            Guid.Parse("b1a2c3d4-0002-4000-8000-000000000002");
    }
}