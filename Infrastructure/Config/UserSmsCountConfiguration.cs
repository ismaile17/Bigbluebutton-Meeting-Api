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
    public class UserSmsCountConfiguration : IEntityTypeConfiguration<UserSmsCount>
    {
        public void Configure(EntityTypeBuilder<UserSmsCount> builder)
        {
            builder.HasKey(a => new { a.UserId });
            builder.HasIndex(a => a.UserId).IsUnique();

        }
    }
}
