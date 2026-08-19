using System;
using MediatR;

namespace KasraLoan.Application.Features.Employee.Queries.GetNextIdentifier
{
    /// <summary>
    /// کد ۹ رقمیِ بعدی (نام کاربری = شماره‌ی پرسنلی) را برای یک استخدام فرضی
    /// در سمت و تاریخ داده‌شده حساب می‌کند، بدون این‌که چیزی ذخیره کند.
    /// برای پیش‌نمایشِ فقط‌خواندنی در فرم «افزودن کاربر» استفاده می‌شود.
    /// </summary>
    public class GetNextIdentifierQuery : IRequest<GetNextIdentifierResponse>
    {
        public int JobPositionId { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class GetNextIdentifierResponse
    {
        public string Identifier { get; set; } = string.Empty;
    }
}
