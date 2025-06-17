using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class CompletedMeetingConfiguration : IEntityTypeConfiguration<CompletedMeeting>
    {
        public void Configure(EntityTypeBuilder<CompletedMeeting> builder)
        {
            builder.Property(s => s.MeetingType).IsRequired();
            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.UserId).IsRequired();
        }
    }
}
