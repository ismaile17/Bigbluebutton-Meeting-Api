using Domain.Entities.Learning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config.LearningConfig
{
    public class LearningMeetingConfiguration : IEntityTypeConfiguration<LearningMeeting>
    {
        public void Configure(EntityTypeBuilder<LearningMeeting> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasMany(m => m.Attendees)
                   .WithOne()
                   .HasForeignKey(a => a.MeetingId);

            builder.HasMany(m => m.Files)
                   .WithOne()
                   .HasForeignKey(f => f.MeetingId);

            builder.HasMany(m => m.Polls)
                   .WithOne()
                   .HasForeignKey(p => p.MeetingId);
        }
    }
}
