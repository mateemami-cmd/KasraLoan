using KasraLoan.Application.DTOs.Loans;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.CreateLoanRequest
{
    public class CreateLoanRequestCommand : IRequest<CreateLoanRequestResponse>
    {
        public CreateLoanRequestDto Request { get; set; } = null!;

        /// <summary>
        /// مدارک پیوست، همراه خودِ درخواست.
        ///
        /// عمداً اینجاست و نه در اندپوینت جدا: برای وام‌هایی که مدرک لازم دارند،
        /// درخواست بدون مدرک اصلاً نباید ساخته شود. اگر آپلود مرحله‌ی بعد بود،
        /// کارمند می‌توانست درخواست بدهد و مدرک را هیچ‌وقت نفرستد.
        /// </summary>
        public List<LoanAttachment> Attachments { get; set; } = new();
    }

    /// <summary>یک فایل پیوست، مستقل از HTTP.</summary>
    public class LoanAttachment
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}