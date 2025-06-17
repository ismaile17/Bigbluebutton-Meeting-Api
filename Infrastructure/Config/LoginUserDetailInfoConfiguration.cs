using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class LoginUserDetailInfoConfiguration : IEntityTypeConfiguration<LoginUserDetailInfo>
    {
        public void Configure(EntityTypeBuilder<LoginUserDetailInfo> builder)
        {
            builder.Property(s => s.MeetingGuid).IsRequired();
        }
    }
}
