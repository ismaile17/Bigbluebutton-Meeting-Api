using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public class MeetingGuestEmailListConfiguration : IEntityTypeConfiguration<MeetingGuestEmailList>
    {
        public void Configure(EntityTypeBuilder<MeetingGuestEmailList> builder)
        {
            // Primary Key
            builder.HasKey(m => new { m.MeetingId, m.Email });

            // Properties
            builder.Property(m => m.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            // Relationships
            builder.HasOne(m => m.Meeting)
                   .WithMany(meeting => meeting.MeetingGuestEmailLists)
                   .HasForeignKey(m => m.MeetingId);
        }
    }
}