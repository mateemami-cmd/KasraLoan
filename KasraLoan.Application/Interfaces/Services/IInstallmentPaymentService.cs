using KasraLoan.Application.DTOs.Loans;
using KasraLoan.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasraLoan.Application.Interfaces.Services
{
    /// <summary>
    /// چرخه‌ی پرداخت قسط: انتخاب روش، پرداخت آنلاین، ثبت چک، و تأیید ادمین.
    ///
    /// قاعده‌ی پنجره: انتخاب روش فقط از روز ۲۵ ماه شمسی تا پایان همان ماه ممکن
    /// است، چون بعد از آن لیست حقوق قطعی می‌شود. هر قسطی که تا آن موقع روشی
    /// برایش انتخاب نشده باشد، کسر از حقوق می‌شود.
    ///
    /// استثنا: قسط معوق را هر زمانی می‌شود پرداخت کرد. بستنِ راه پرداخت روی
    /// بدهکار به نفع هیچ‌کس نیست.
    /// </summary>
    public interface IInstallmentPaymentService
    {
        /// <summary>قسط بعدیِ پرداخت‌نشده‌ی کارمند و وضعیت پنجره‌ی انتخاب.</summary>
        Task<CurrentInstallmentDto> GetCurrentInstallmentAsync(Guid employeeId);

        /// <summary>ثبت انتخاب روش پرداخت برای یک قسط.</summary>
        Task<InstallmentPaymentDto> SelectMethodAsync(
            Guid installmentId,
            Guid employeeId,
            PaymentMethod method);

        /// <summary>ثبت چک همراه تصویر؛ نتیجه‌اش رفتن به صف تأیید ادمین است.</summary>
        Task<InstallmentPaymentDto> SubmitChequeAsync(
            Guid installmentId,
            Guid employeeId,
            SubmitChequeRequestDto info,
            byte[] imageBytes,
            string fileName,
            string contentType);

        /// <summary>باز کردن نشستِ پرداخت آنلاین.</summary>
        Task<GatewaySessionDto> StartGatewayPaymentAsync(Guid installmentId, Guid employeeId);

        /// <summary>اطلاعات نمایشی یک نشست پرداخت (بدون داده‌ی حساس).</summary>
        Task<GatewaySessionDto> GetGatewaySessionAsync(Guid authority);

        /// <summary>
        /// نهایی کردن پرداخت آنلاین. اطلاعات کارت فقط اعتبارسنجی می‌شوند و
        /// هرگز ذخیره یا لاگ نمی‌شوند.
        /// </summary>
        Task<InstallmentPaymentDto> CompleteGatewayPaymentAsync(
            Guid authority,
            GatewayPaymentRequestDto card);

        /// <summary>صف چک‌های منتظر تأیید (ادمین).</summary>
        Task<List<InstallmentPaymentDto>> GetPendingChequesAsync();

        /// <summary>تأیید چک توسط ادمین؛ قسط تسویه می‌شود.</summary>
        Task<InstallmentPaymentDto> ConfirmChequeAsync(Guid paymentId, Guid adminId);

        /// <summary>رد چک؛ قسط به حالت پرداخت‌نشده برمی‌گردد.</summary>
        Task<InstallmentPaymentDto> RejectChequeAsync(Guid paymentId, Guid adminId, string reason);
    }
}
