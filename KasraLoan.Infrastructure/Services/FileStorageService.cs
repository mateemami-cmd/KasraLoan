using KasraLoan.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasraLoan.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> SaveFileAsync(
            byte[] file,
            string fileName,
            string contentType)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(fileName);

            var uniqueFileName =
                $"{Guid.NewGuid()}{extension}";

            var fullPath =
                Path.Combine(uploadsFolder, uniqueFileName);

            await File.WriteAllBytesAsync(fullPath, file);

            return $"/uploads/{uniqueFileName}";
        }
    }
}