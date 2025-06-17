using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public class MeetingGroupConfiguration : IEntityTypeConfiguration<MeetingGroup>
    {
        public void Configure(EntityTypeBuilder<MeetingGroup> builder)
        {
            builder.Property(s => s.Name).IsRequired().HasMaxLength(1500);


        }
    }
}