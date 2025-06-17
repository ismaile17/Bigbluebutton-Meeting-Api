using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config
{
    internal class LandingContactFormConfiguration : IEntityTypeConfiguration<LandingContactForm>
    {
        public void Configure(EntityTypeBuilder<LandingContactForm> builder)
        {
            builder.Property(s => s.Email).IsRequired();
        }
    }
}
