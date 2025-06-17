using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    public class LandingCampaignEmailConfiguration : IEntityTypeConfiguration<LandingCampaignEmail>
    {
        public void Configure(EntityTypeBuilder<LandingCampaignEmail> builder)
        {
            builder.Property(s => s.Email).IsRequired();
        }
    }
}
