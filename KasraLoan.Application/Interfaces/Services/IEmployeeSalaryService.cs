using KasraLoan.Domain.Entities;

namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// حقوق مؤثر کارمند و سقف قسط ماهانه‌ی او.
    /// هم‌خانواده‌ی <see cref="IEmployeeScoreService"/>: همان‌طور که امتیاز می‌تواند
    /// override دستی داشته باشد، حقوق هم می‌تواند از حقوق پایه‌ی سمت شغلی جدا باشد.
    /// </summary>
    public interface IEmployeeSalaryService
    {
        /// <summary>
        /// سقف مجاز کسر ماهانه از حقوق، به‌صورت درصد (نسبت قسط به درآمد / DTI).
        /// یک‌سوم حقوق؛ هم عرف صندوق‌های همیار است و هم با سقف قانونی کسر از حقوق
        /// در ایران (حداکثر یک‌سوم) هم‌خوانی دارد.
        /// </summary>
        decimal MaxInstallmentToSalaryPercent { get; }

        /// <summary>
        /// حقوق ماهانه‌ی مؤثر: اگر کارمند حقوق اختصاصی داشته باشد همان، وگرنه
        /// حقوق پایه‌ی سمت شغلی‌اش. اگر هیچ‌کدام نباشد صفر برمی‌گرداند.
        /// نکته: برای درست کار کردن، <see cref="Employee.JobPosition"/> باید
        /// از قبل Include شده باشد.
        /// </summary>
        long GetEffectiveMonthlySalary(Employee employee);

        /// <summary>بیشترین مبلغی که ماهانه می‌توان بابت قسط از حقوق کارمند کسر کرد.</summary>
        decimal GetMaxMonthlyInstallment(Employee employee);
    }
}
