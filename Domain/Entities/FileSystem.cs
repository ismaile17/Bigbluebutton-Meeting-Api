using Shared.Enum;

namespace Domain.Entities
{
    public class FileSystem : BaseEntity
    {
        public FileSystemPageType PageType { get; set; }
        public int PageId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string FileKey { get; set; }
        public long FileSize { get; set; }
        public string? FileUrl { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? UploadedIpAddress { get; set; }
        public int? UploadedPort { get; set; }
        public string? DeletedIpAddress { get; set; }
        public int? DeletedPort { get; set; }
    }
}