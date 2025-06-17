using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
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
    public class DeleteAwsFileCommand : IRequest<ResultSingle<FileSystemAnswerDto>>
    {
        public int FileId { get; set; } // Silinecek dosyanın ID'si
        public int AppUserId { get; set; }    // İşlemi yapan kullanıcının ID'si
    }

    public class DeleteAwsFileCommandHandler : IRequestHandler<DeleteAwsFileCommand, ResultSingle<FileSystemAnswerDto>>
    {
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettingsDto _awsSettings;
        private readonly IClientInfoService _clientInfoService;

        public DeleteAwsFileCommandHandler(IRepository<Domain.Entities.FileSystem> fileSystemRepository, IAmazonS3 s3Client, IOptions<AwsSettingsDto> awsSettings, IClientInfoService clientInfoService)
        {
            _fileSystemRepository = fileSystemRepository;
            _s3Client = s3Client;
            _awsSettings = awsSettings.Value;
            _clientInfoService = clientInfoService;
        }

        public async Task<ResultSingle<FileSystemAnswerDto>> Handle(DeleteAwsFileCommand request, CancellationToken cancellationToken)
        {
            // FileSystem kaydını alıyoruz
            var fileSystem = _fileSystemRepository.Get(a => a.Id == request.FileId && a.CreatedBy == request.AppUserId);

            if (fileSystem == null)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "File not found.");
            }

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();


            // AWS S3'ten dosyayı siliyoruz
            try
            {
                // AWS kimlik bilgilerini ayarlıyoruz
                var credentials = new BasicAWSCredentials(_awsSettings.AccessKey, _awsSettings.SecretKey);
                var region = Amazon.RegionEndpoint.GetBySystemName(_awsSettings.Region);

                using var s3Client = new AmazonS3Client(credentials, region);

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = GetBucketName(fileSystem.PageType),
                    Key = fileSystem.FileKey
                };

                await s3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                return Result.Fail<FileSystemAnswerDto>(null, $"AWS error: {e.Message}");
            }

            // Veritabanında dosyayı silinmiş olarak işaretliyoruz
            fileSystem.IsDeleted = true;
            fileSystem.DeletedDate = DateTime.Now;
            fileSystem.UpdatedBy = request.AppUserId;
            fileSystem.UpdatedTime = DateTime.Now;
            fileSystem.DeletedIpAddress = ipAddress;
            fileSystem.DeletedPort = port;

            _fileSystemRepository.Update(fileSystem);
            var result = await _fileSystemRepository.CommitAsync(cancellationToken);

            if (result == -1)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "File record could not be updated.");
            }

            return Result.Ok(new FileSystemAnswerDto
            {
                FileSystemAnswerType = 3,
                FileSystemAnswer = true
            }, "File successfully deleted.");
        }

        // Bucket adını belirliyoruz
        public string GetBucketName(FileSystemPageType pageType)
        {
            return pageType switch
            {
                FileSystemPageType.GroupTeacher => "groupteacher",
                FileSystemPageType.GroupParticipantHomework => "groupparticipant",
                FileSystemPageType.Logo => "meetinglogo",
                FileSystemPageType.TaskAssigment => "taskassigment",
                _ => throw new ArgumentException("Invalid FileSystemPageType value"),
            };
        }
    }

}