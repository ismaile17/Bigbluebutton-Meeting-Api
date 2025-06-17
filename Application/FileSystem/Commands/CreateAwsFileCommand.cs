using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Application.Cupon.Service;
using Application.FileSystem.Models;
using Application.FileSystem.Services;
using Application.Services.ClientInfoService;
using Application.Shared.Results;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Enum;

namespace Application.FileSystem.Commands
{
    public class CreateAwsFileCommand : IRequest<ResultSingle<FileSystemAnswerDto>>
    {
        public int AppUserId { get; set; }
        public string FileName { get; set; }
        public Stream FileStream { get; set; }
        public string ContentType { get; set; }
        public string FileType { get; set; }
        public int PageId { get; set; }
        public FileSystemPageType PageType { get; set; }
    }

    public class CreateAwsFileCommandHandler : IRequestHandler<CreateAwsFileCommand, ResultSingle<FileSystemAnswerDto>>
    {
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettingsDto _awsSettings;
        private readonly IClientInfoService _clientInfoService;
        private readonly FileSystemAuthControlService _fileSystemAuthControlService;
        private readonly IMediator _mediator;

        public CreateAwsFileCommandHandler(IRepository<Domain.Entities.FileSystem> fileSystemRepository, 
            IAmazonS3 s3Client,
            IOptions<AwsSettingsDto> awsSettings,
            IClientInfoService clientInfoService, 
            FileSystemAuthControlService fileSystemAuthControlService, IMediator mediator)
        {
            _fileSystemRepository = fileSystemRepository;
            _s3Client = s3Client;
            _awsSettings = awsSettings.Value;
            _clientInfoService = clientInfoService;
            _fileSystemAuthControlService = fileSystemAuthControlService;
            _mediator = mediator;
        }

        public async Task<ResultSingle<FileSystemAnswerDto>> Handle(CreateAwsFileCommand request, CancellationToken cancellationToken)
        {
            var bucketName = GetBucketName(request.PageType);

            var fileKey = Guid.NewGuid().ToString() + "-" + request.FileName;

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();


            const long maxFileSize = 1L * 1024 * 1024 * 1024;

            if (request.FileStream.Length > maxFileSize)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "File size exceeds the 1 GB limit.");
            }

            var isAuthorized = await _fileSystemAuthControlService.FileAuthControl(
                request.AppUserId,
                request.PageType,
                request.PageId,
                isDelete: false,
                cancellationToken);

            if (!isAuthorized)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "You are not authorized to perform this action.");
            }


            try
            {
                var credentials = new BasicAWSCredentials(_awsSettings.AccessKey, _awsSettings.SecretKey);
                var region = Amazon.RegionEndpoint.GetBySystemName(_awsSettings.Region);

                using (var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
                        {
                            RegionEndpoint = region,
                            SignatureVersion = "v4",
                        }))

                {
                    var fileTransferUtility = new TransferUtility(s3Client);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = request.FileStream,
                        Key = fileKey,
                        BucketName = bucketName,
                        ContentType = request.ContentType
                    };

                    await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);
                }

                var fileSystem = new Domain.Entities.FileSystem
                {
                    PageId = request.PageId,
                    PageType = request.PageType,
                    FileName = request.FileName,
                    ContentType = request.ContentType,
                    FileKey = fileKey,
                    FileSize = request.FileStream.Length,
                    FileUrl = $"https://{bucketName}.s3.amazonaws.com/{fileKey}",
                    IsDeleted = false,
                    CreatedBy = request.AppUserId,
                    UploadedIpAddress = ipAddress,
                    UploadedPort = port
                };

                _fileSystemRepository.InsertWithoutCommit(fileSystem);
                var result = await _fileSystemRepository.CommitAsync(cancellationToken);
                if (result == -1)
                {
                    return Result.Fail<FileSystemAnswerDto>(null, "File record could not be saved.");
                }

                var fileSystemAnswerDto = new FileSystemAnswerDto
                {
                    FileSystemAnswerType = 1, // İşlem başarılıysa 1
                    FileSystemAnswer = true,
                    LogoUrl = request.PageType == FileSystemPageType.Logo ? fileSystem.FileUrl : null
                };


                return Result.Ok(fileSystemAnswerDto, "File uploaded and saved successfully.");
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
                FileSystemPageType.TaskAssigment => "taskassigment",
                FileSystemPageType.TaskSubmission => "tasksubmission",
                _ => throw new ArgumentException("Invalid FileSystemPageType value"),
            };
        }
    }
}