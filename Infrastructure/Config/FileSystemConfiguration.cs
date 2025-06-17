using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class FileSystemConfiguration : IEntityTypeConfiguration<FileSystem>
    {
        public void Configure(EntityTypeBuilder<FileSystem> builder)
        {
            builder.Property(s=>s.FileKey).IsRequired();
        }
    }
}
