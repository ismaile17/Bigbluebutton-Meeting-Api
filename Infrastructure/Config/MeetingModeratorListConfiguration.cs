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
    public class MeetingModeratorListConfiguration : IEntityTypeConfiguration<MeetingModeratorList>
    {
        public void Configure(EntityTypeBuilder<MeetingModeratorList> builder)
        {
            builder
                .HasKey(a => new { a.MeetingId, a.AppUserId });
            builder
                .HasOne(a => a.Meeting)
                .WithMany(b => b.MeetingModeratorLists)
                .HasForeignKey(b => b.MeetingId);
            builder
                .HasOne(a => a.AppUser)
                .WithMany(c => c.MeetingModeratorLists)
                .HasForeignKey(a => a.AppUserId);
        }
    }
}
