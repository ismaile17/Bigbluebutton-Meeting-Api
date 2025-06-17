using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class BBBServerConfiguration : IEntityTypeConfiguration<BBBServer>
    {
        public void Configure(EntityTypeBuilder<BBBServer> builder)
        {
            builder.Property(s=>s.ServerApiUrl).IsRequired();
            builder.Property(s=>s.SharedSecret).IsRequired();            
        }
    }
}
