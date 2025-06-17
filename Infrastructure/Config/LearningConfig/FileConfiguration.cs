using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Learning;

public class FileConfiguration : IEntityTypeConfiguration<LearningFile>
{
    public void Configure(EntityTypeBuilder<LearningFile> builder)
    {
        builder.HasKey(f => f.Id);
    }
}
