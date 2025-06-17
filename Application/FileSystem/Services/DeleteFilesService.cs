using Amazon.S3;
using Amazon.S3.Model;
using Application.FileSystem.Models;
using Application.Services.ClientInfoService;
using Application.Shared.Results;
using Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Shared.Enum;

namespace Application.FileSystem.Services
{
    public interface IDeleteFilesService
    {
        Task<Result> DeleteFilesAsync(List<int> fileIds, int appUserId, CancellationToken cancellationToken);
    }

    public class DeleteFilesService : IDeleteFilesService
    {
        private readonly IRepository<Domain.Entities.FileSystem> _fileSystemRepository;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettingsDto _awsSettings;
        private readonly IClientInfoService _clientInfoService;

        public DeleteFilesService(
            IRepository<Domain.Entities.FileSystem> fileSystemRepository,
            IAmazonS3 s3Client,
            IOptions<AwsSettingsDto> awsSettings,
            IClientInfoService clientInfoService)
        {
            _fileSystemRepository = fileSystemRepository;
            _s3Client = s3Client;
            _awsSettings = awsSettings.Value;
            _clientInfoService = clientInfoService;
        }

        /// <summary>
        /// Belirtilen dosya ID'lerini kullanarak AWS S3'ten dosyaları siler ve veritabanındaki kayıtları günceller.
        /// </summary>
        /// <param name="fileIds">Silinecek dosyaların ID'leri</param>
        /// <param name="appUserId">İşlemi yapan kullanıcının ID'si</param>
        /// <param name="cancellationToken">İptal token'ı</param>
        /// <returns>İşlem sonucu</returns>
        public async Task<Result> DeleteFilesAsync(List<int> fileIds, int appUserId, CancellationToken cancellationToken)
        {
            if (fileIds == null || !fileIds.Any())
            {
                return new Result(false, "Silinecek dosya ID'leri listesi boş.");
            }

            // Silinecek dosyaların veritabanından alınması
            var files = _fileSystemRepository.GetAll
                .Where(f => fileIds.Contains(f.Id) && !f.IsDeleted)
                .ToList();

            if (files == null || !files.Any())
            {
                return new Result(false, "Hiçbir dosya bulunamadı.");
            }

            var (ipAddress, port) = _clientInfoService.GetClientIpAndPort();

            // Dosyaları bucket bazında gruplayarak toplu silme işlemi
            var filesByBucket = files.GroupBy(f => GetBucketName(f.PageType));

            try
            {
                foreach (var bucketGroup in filesByBucket)
                {
                    var deleteObjectsRequest = new DeleteObjectsRequest
                    {
                        BucketName = bucketGroup.Key,
                        Objects = bucketGroup.Select(f => new KeyVersion { Key = f.FileKey }).ToList(),
                        Quiet = false
                    };

                    var response = await _s3Client.DeleteObjectsAsync(deleteObjectsRequest, cancellationToken);

                    if (response.DeleteErrors.Any())
                    {
                        // Hataları loglayabilir veya uygun şekilde işleyebilirsiniz
                        var errorMessages = string.Join(", ", response.DeleteErrors.Select(e => e.Message));
                        return new Result(false, $"AWS S3 hatası: {errorMessages}");
                    }
                }
            }
            catch (AmazonS3Exception e)
            {
                return new Result(false, $"AWS hata: {e.Message}");
            }

            // Veritabanında dosyaları silinmiş olarak işaretleme
            foreach (var file in files)
            {
                file.IsDeleted = true;
                file.DeletedDate = DateTime.UtcNow;
                file.UpdatedBy = appUserId;
                file.UpdatedTime = DateTime.UtcNow;
                file.DeletedIpAddress = ipAddress;
                file.DeletedPort = port;

                _fileSystemRepository.Update(file);
            }

            var commitResult = await _fileSystemRepository.CommitAsync(cancellationToken);

            if (commitResult == -1)
            {
                return new Result(false, "Dosya kayıtları güncellenemedi.");
            }

            return new Result(true, "Dosyalar başarıyla silindi.");
        }

        /// <summary>
        /// Dosyanın sayfa tipine göre bucket adını belirler.
        /// </summary>
        /// <param name="pageType">Dosyanın sayfa tipi</param>
        /// <returns>Bucket adı</returns>
        /// <exception cref="ArgumentException">Geçersiz sayfa tipi değeri</exception>
        private string GetBucketName(FileSystemPageType pageType)
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
