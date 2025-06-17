using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config.LearningConfig
{
    public class PostArchiveForMeetingVideoConfiguration : IEntityTypeConfiguration<PostArchiveForMeetingVideo>
    {
        public void Configure(EntityTypeBuilder<PostArchiveForMeetingVideo> builder)
        {
            builder.HasKey(ld => ld.Id);
        }
    }
}
