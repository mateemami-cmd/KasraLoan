using KasraLoan.Application.Common.Exceptions;
using KasraLoan.Application.Interfaces.Repositories;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KasraLoan.Application.Features.Employee.Commands.UploadProfilePicture
{
    public class UploadProfilePictureHandler
        : IRequestHandler<UploadProfilePictureCommand, UploadProfilePictureResponse>
    {
        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/webp" };

        private const int MaxSizeBytes = 3 * 1024 * 1024; // 3 مگابایت

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrentUserService _currentUserService;

        public UploadProfilePictureHandler(
            IEmployeeRepository employeeRepository,
            IFileStorageService fileStorageService,
            ICurrentUserService currentUserService)
        {
            _employeeRepository = employeeRepository;
            _fileStorageService = fileStorageService;
            _currentUserService = currentUserService;
        }

        public async Task<UploadProfilePictureResponse> Handle(
            UploadProfilePictureCommand request,
            CancellationToken cancellationToken)
        {
            if (request.FileContent.Length == 0)
                throw new BusinessRuleException("فایلی انتخاب نشده است.");

            if (request.FileContent.Length > MaxSizeBytes)
                throw new BusinessRuleException("حجم عکس نباید بیشتر از ۳ مگابایت باشد.");

            if (!AllowedContentTypes.Contains(request.ContentType))
                throw new BusinessRuleException("فقط فرمت‌های JPG، PNG و WEBP مجاز هستند.");

            var employee = await _employeeRepository.GetByIdAsync(_currentUserService.UserId);

            if (employee == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            var url = await _fileStorageService.SaveFileAsync(
                request.FileContent,
                request.FileName,
                request.ContentType);

            employee.ProfilePictureUrl = url;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new UploadProfilePictureResponse
            {
                ProfilePictureUrl = url,
                Message = "عکس پروفایل با موفقیت به‌روزرسانی شد."
            };
        }
    }
}
