using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Application.FileSystem.Models;
using Application.FileSystem.Services;
using Application.Shared.Results;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Enum;

namespace Application.FileSystem.Commands
{
    public class CreateDownloadAwsLinkCommand : IRequest<ResultSingle<FileSystemAnswerDto>>
    {
        public int FileId { get; set; } 
        public int AppUserId { get; set; }
    }

    public class CreateDownloadAwsLinkCommandHandler : IRequestHandler<CreateDownloadAwsLinkCommand, ResultSingle<FileSystemAnswerDto>>
    {
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IRepository<ManagerParticipant> _managerParticipantRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettingsDto _awsSettings;
        private readonly FileSystemAuthControlService _fileSystemAuthControlService;

        public CreateDownloadAwsLinkCommandHandler(
            IRepository<Domain.Entities.FileSystem> fileSystemRepository,
            IRepository<ManagerParticipant> managerParticipantRepository,
            UserManager<AppUser> userManager,
            IAmazonS3 s3Client,
            IOptions<AwsSettingsDto> awsSettingsOptions,
            FileSystemAuthControlService fileSystemAuthControlService)
        {
            _fileSystemRepository = fileSystemRepository;
            _managerParticipantRepository = managerParticipantRepository;
            _userManager = userManager;
            _s3Client = s3Client;
            _awsSettings = awsSettingsOptions.Value;
            _fileSystemAuthControlService = fileSystemAuthControlService;
        }

        public async Task<ResultSingle<FileSystemAnswerDto>> Handle(CreateDownloadAwsLinkCommand request, CancellationToken cancellationToken)
        {
            var fileSystem = _fileSystemRepository.Get(a => a.Id == request.FileId);
            if (fileSystem == null)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "Dosya bulunamadı.");
            }

            // Kullanıcıyı alıyoruz
            var currentUser = await _userManager.FindByIdAsync(request.AppUserId.ToString());
            if (currentUser == null)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "Kullanıcı bulunamadı.");
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            bool isAuthorized = false;

            if (roles.Contains("Teacher") || roles.Contains("Admin"))
            {
                if (fileSystem.CreatedBy == request.AppUserId)
                {
                    isAuthorized = true;
                }
                else
                {
                    var managerParticipant = _managerParticipantRepository.Get(
                        mp => mp.ManagerId == request.AppUserId
                        && mp.ParticipantId == fileSystem.CreatedBy
                        && mp.IsActive == true);

                    if (managerParticipant != null)
                    {
                        isAuthorized = true;
                    }
                }
            }
            else
            {
                if (fileSystem.CreatedBy == request.AppUserId)
                {
                    isAuthorized = true;
                }
            }
            if(!isAuthorized && (fileSystem.PageType == FileSystemPageType.TaskAssigment || fileSystem.PageType == FileSystemPageType.GroupTeacher))
            {
                var managerParticipant = _managerParticipantRepository.Get(
                        mp => mp.ManagerId == fileSystem.CreatedBy
                        && mp.ParticipantId == request.AppUserId
                        && mp.IsActive == true);
                if (managerParticipant != null)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
            {
                return Result.Fail<FileSystemAnswerDto>(null, "Bu işlemi yapma yetkiniz yok.");
            }

            try
            {
                var credentials = new BasicAWSCredentials(_awsSettings.AccessKey, _awsSettings.SecretKey);
                var region = Amazon.RegionEndpoint.GetBySystemName(_awsSettings.Region);

                var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
                {
                    SignatureVersion = "v4",
                    RegionEndpoint = region
                });

                var requestLink = new GetPreSignedUrlRequest
                {
                    BucketName = GetBucketName(fileSystem.PageType),
                    Key = fileSystem.FileKey,
                    Expires = DateTime.UtcNow.AddHours(1),
                    ResponseHeaderOverrides = new ResponseHeaderOverrides
                    {
                        ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileSystem.FileName)}"
                    }
                };

                var url = s3Client.GetPreSignedURL(requestLink);

                return Result.Ok(new FileSystemAnswerDto
                {
                    FileSystemAnswerType = 2,
                    FileSystemAnswer = true
                }, url);
            }
            catch (AmazonS3Exception e)
            {
                return Result.Fail<FileSystemAnswerDto>(null, $"Hata: {e.Message}");
            }
        }
        public string GetBucketName(FileSystemPageType pageType)
        {
            return pageType switch
            {
                FileSystemPageType.GroupTeacher => "groupteacher",
                FileSystemPageType.GroupParticipantHomework => "groupparticipant",
                FileSystemPageType.Logo => "meetinglogo",
                FileSystemPageType.TaskAssigment => "taskassigment",
                FileSystemPageType.TaskSubmission => "tasksubmission",
                _ => throw new ArgumentException("Geçersiz FileSystemPageType değeri"),
            };
        }
    }
}
