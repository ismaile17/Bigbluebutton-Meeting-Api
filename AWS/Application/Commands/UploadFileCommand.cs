using AWS.Services;
using Helper.Results;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class UploadFileCommand : IRequest<Result<string>>
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<string>>
    {
        private readonly S3Service _s3Service;
        private readonly string _bucketName = "your-bucket-name";

        public UploadFileCommandHandler(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public async Task<Result<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            if (request.FileStream == null || string.IsNullOrEmpty(request.FileName))
            {
                return Result<string>.Failure("Dosya veya dosya adı boş olamaz.");
            }

            string fileKey;
            try
            {
                fileKey = await _s3Service.UploadFileAsync(request.FileStream, request.FileName, _bucketName);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Dosya yükleme başarısız: {ex.Message}");
            }

            return Result<string>.Success(fileKey);
        }
    }
}
