using Domain.Entities.Learning;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class AttendeeSessionConfiguration : IEntityTypeConfiguration<AttendeeSession>
{
    public void Configure(EntityTypeBuilder<AttendeeSession> builder)
    {
        builder.HasKey(s => s.Id);
    }
}
