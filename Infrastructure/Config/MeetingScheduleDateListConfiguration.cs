using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class MeetingScheduleDateListConfiguration : IEntityTypeConfiguration<MeetingScheduleDateList>
    {
        public void Configure(EntityTypeBuilder<MeetingScheduleDateList> builder)
        {
            // Primary Key
            builder.HasKey(m => new { m.MeetingId, m.Date });

            // Relationships
            builder.HasOne(m => m.Meeting)
                   .WithMany(meeting => meeting.MeetingScheduleDateLists)
                   .HasForeignKey(m => m.MeetingId);
        }
    }
}