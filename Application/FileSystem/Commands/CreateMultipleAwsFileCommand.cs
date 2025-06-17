using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Application.FileSystem.Models;
using Application.Services.ClientInfoService;
using Application.Shared.Results;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.Enum;

namespace Application.FileSystem.Commands
{
    public class CreateMultipleAwsFileCommand : IRequest<ResultSingle<FileSystemAnswerDto>>
    {
        public int AppUserId { get; set; }
        public string FileName { get; set; }
        public List<IFormFile> Files { get; set; } // Birden fazla dosya için liste
        public string ContentType { get; set; }
        public string FileType { get; set; }
        public int PageId { get; set; }
        public FileSystemPageType PageType { get; set; }
    }

    public class CreateMultipleAwsFileCommandHandler : IRequestHandler<CreateMultipleAwsFileCommand, ResultSingle<FileSystemAnswerDto>>
    {
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettingsDto _awsSettings;
        private readonly IClientInfoService _clientInfoService;

        public CreateMultipleAwsFileCommandHandler(IRepository<Domain.Entities.FileSystem> fileSystemRepository, IAmazonS3 s3Client, IOptions<AwsSettingsDto> awsSettings, IClientInfoService clientInfoService)
        {
            _fileSystemRepository = fileSystemRepository;
            _s3Client = s3Client;
            _awsSettings = awsSettings.Value;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<FileSystemAnswerDto>> Handle(CreateMultipleAwsFileCommand request, CancellationToken cancellationToken)
        {
            var bucketName = GetBucketName(request.PageType);
            var allFilesUploadedSuccessfully = true;

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();


            const long maxTotalFileSize = 1L * 1024 * 1024 * 1024;
            long totalSize = 0;

            foreach (var file in request.Files)
            {
                totalSize += file.Length;
                if (totalSize > maxTotalFileSize)
                {
                    return Result.Fail<FileSystemAnswerDto>(null, "The total size of files exceeds the 1 GB limit.");
                }
            }

            try
            {
                var credentials = new BasicAWSCredentials(_awsSettings.AccessKey, _awsSettings.SecretKey);
                var region = Amazon.RegionEndpoint.GetBySystemName(_awsSettings.Region);

                // S3 Client'i bir kere oluşturuyoruz
                using var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
                {
                    RegionEndpoint = region,
                    SignatureVersion = "v4"
                });

                var fileTransferUtility = new TransferUtility(s3Client);

                foreach (var file in request.Files)
                {
                    var fileKey = Guid.NewGuid().ToString() + "-" + file.FileName;

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = file.OpenReadStream(),
                        Key = fileKey,
                        BucketName = bucketName,
                        ContentType = file.ContentType
                    };

                    await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

                    var fileSystem = new Domain.Entities.FileSystem
                    {
                        PageId = request.PageId,
                        PageType = request.PageType,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileKey = fileKey,
                        FileSize = file.Length,
                        FileUrl = $"https://{bucketName}.s3.amazonaws.com/{fileKey}",
                        IsDeleted = false,
                        CreatedBy = request.AppUserId,
                        UploadedIpAddress = ipAddress,
                        UploadedPort = port
                    };

                    _fileSystemRepository.InsertWithoutCommit(fileSystem);
                }

                var result = await _fileSystemRepository.CommitAsync(cancellationToken);
                if (result == -1)
                {
                    return Result.Fail<FileSystemAnswerDto>(null, "Files could not be saved.");
                }

                return Result.Ok(new FileSystemAnswerDto
                {
                    FileSystemAnswerType = 1, // İşlem başarılıysa 1
                    FileSystemAnswer = true
                }, "Files uploaded and saved successfully.");
            }
            catch (AmazonS3Exception e)
            {
                return Result.Fail<FileSystemAnswerDto>(null, $"AWS error: {e.Message}");
            }
            catch (Exception e)
            {
                return Result.Fail<FileSystemAnswerDto>(null, $"Error: {e.Message}");
            }
        }

        // Her PageType için bucket adını belirliyoruz
        public string GetBucketName(FileSystemPageType pageType)
        {
            return pageType switch
            {
                FileSystemPageType.GroupTeacher => "groupteacher",
                FileSystemPageType.GroupParticipantHomework => "groupparticipant",
                FileSystemPageType.Logo => "meetinglogo",
                _ => throw new ArgumentException("Invalid FileSystemPageType value"),
            };
        }
    }

}
