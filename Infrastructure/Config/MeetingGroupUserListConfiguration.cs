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
    public class MeetingGroupUserListConfiguration : IEntityTypeConfiguration<MeetingGroupUserList>
    {
        public void Configure(EntityTypeBuilder<MeetingGroupUserList> builder)
        {
            builder
                  .HasKey(a => new { a.MeetingGroupId, a.AppUserId });
            builder
                .HasOne(a => a.MeetingGroup)
                .WithMany(b => b.MeetingGroupUserLists)
                .HasForeignKey(a => a.MeetingGroupId);
            builder
                .HasOne(a => a.AppUser)
                .WithMany(c => c.MeetingGroupUserLists)
                .HasForeignKey(a => a.AppUserId);
        }
    }
}