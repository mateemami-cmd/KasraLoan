using MediatR;
using System;

namespace KasraLoan.Application.Features.Employee.Commands.UploadProfilePicture
{
    public class UploadProfilePictureCommand : IRequest<UploadProfilePictureResponse>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
