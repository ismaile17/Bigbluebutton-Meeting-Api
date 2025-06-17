using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class MeetingMeetingGroupConfiguration : IEntityTypeConfiguration<MeetingMeetingGroup>
    {
        public void Configure(EntityTypeBuilder<MeetingMeetingGroup> builder)
        {
            builder
                  .HasKey(a => new { a.MeetingGroupId, a.MeetingId });
            builder
                .HasOne(a => a.MeetingGroup)
                .WithMany(b => b.MeetingMeetingGroups)
                .HasForeignKey(a => a.MeetingGroupId);
            builder
                .HasOne(a => a.Meeting)
                .WithMany(c => c.MeetingMeetingGroups)
                .HasForeignKey(a => a.MeetingId);

        }
    }
}
