using System;
using System.Collections.Generic;

namespace KasraLoan.Domain.Entities
{
    /// <summary>
    /// سمت شغلی کارمند به‌همراه حقوق پایه‌ی آن سمت.
    ///
    /// حقوق پایه عمداً در دیتابیس نگهداری می‌شود و در کد هاردکد نشده، چون هر سال
    /// تغییر می‌کند و نباید برای تغییرش نیاز به انتشار مجدد برنامه باشد.
    /// اگر حقوق واقعی کارمندی با حقوق پایه‌ی سمتش فرق داشته باشد، مقدار واقعی در
    /// <see cref="Employee.MonthlySalary"/> ذخیره می‌شود و بر این مقدار اولویت دارد.
    /// </summary>
    public class JobPosition
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>حقوق پایه‌ی ماهانه به تومان.</summary>
        public long BaseSalary { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
