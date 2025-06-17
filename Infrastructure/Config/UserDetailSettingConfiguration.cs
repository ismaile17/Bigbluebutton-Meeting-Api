using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class UserDetailSettingConfiguration : IEntityTypeConfiguration<UserDetailSetting>
    {
        public void Configure(EntityTypeBuilder<UserDetailSetting> builder)
        {
            builder.HasKey(a => a.UserId);

            builder.HasOne(uds => uds.User)
                   .WithOne(u => u.UserDetailSetting)
                   .HasForeignKey<UserDetailSetting>(uds => uds.UserId);

            builder.HasIndex(a => a.UserId).IsUnique();
        }
    }
}
