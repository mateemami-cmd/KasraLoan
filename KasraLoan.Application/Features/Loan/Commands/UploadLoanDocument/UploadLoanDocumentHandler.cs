using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Loan.Commands.UploadLoanDocument
{
    public class UploadLoanDocumentHandler : IRequestHandler<UploadLoanDocumentCommand, UploadLoanDocumentResponse>
    {
        private readonly ILoanRequestRepository _loanRequestRepository;
        private readonly ILoanDocumentRepository _loanDocumentRepository;
        private readonly IFileStorageService _fileStorageService;

        public UploadLoanDocumentHandler(ILoanRequestRepository loanRequestRepository, ILoanDocumentRepository loanDocumentRepository, IFileStorageService fileStorageService)
        {
            _loanRequestRepository = loanRequestRepository;
            _loanDocumentRepository = loanDocumentRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<UploadLoanDocumentResponse> Handle(UploadLoanDocumentCommand request, CancellationToken cancellationToken)
        {
            var loan = await _loanRequestRepository
                .GetByIdAsync(request.LoanRequestId);

            if (loan == null)
                throw new KeyNotFoundException("وام یافت نشد");

            var exists = await _loanDocumentRepository.ExistsAsync(request.LoanRequestId);

            if (exists)
            {
                return new UploadLoanDocumentResponse
                {
                    IsSuccess = false,
                    Message = "برای این وام قبلاً مدرک ثبت شده است."
                };
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (request.FileContent.Length > maxFileSize)
            {
                return new UploadLoanDocumentResponse
                {
                    IsSuccess = false,
                    Message = "حداکثر حجم فایل 5 مگابایت است."
                };
            }

            var extension = Path.GetExtension(request.FileName).ToLower();

            var allowedExtensions = new[]
            {".jpg",".jpeg",".png",".pdf"
            };

            if (!allowedExtensions.Contains(extension))
            {
                return new UploadLoanDocumentResponse
                {
                    IsSuccess = false,
                    Message = "فرمت فایل مجاز نیست."
                };
            }

            var filePath = await _fileStorageService.SaveFileAsync(
                request.FileContent,
                request.FileName,
                request.ContentType);

            var document = new LoanDocument
            {
                LoanRequestId = loan.Id,
                FileName = request.FileName,
                FilePath = filePath,
                UploadedAt = DateTime.UtcNow
            };

            await _loanDocumentRepository.AddAsync(document);

            await _loanDocumentRepository.SaveChangesAsync();

            return new UploadLoanDocumentResponse
            {
                Message = "فایل با موفقیت آپلود شد"
            };
        }
    }
}