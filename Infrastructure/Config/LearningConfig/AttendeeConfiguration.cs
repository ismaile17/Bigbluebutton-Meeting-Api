using Domain.Entities.Learning;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class AttendeeConfiguration : IEntityTypeConfiguration<Attendee>
{
    public void Configure(EntityTypeBuilder<Attendee> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasMany(a => a.Sessions)
               .WithOne()
               .HasForeignKey(s => s.AttendeeId);
    }
}
